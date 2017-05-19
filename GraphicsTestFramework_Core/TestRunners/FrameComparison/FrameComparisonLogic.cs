using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace GraphicsTestFramework
{
    public class FrameComparisonLogic : TestLogic<FrameComparisonModel>
    {
        /// ------------------------------------------------------------------------------------
        /// Logic specific variables

        Camera dummyCamera;
        RenderTexture temporaryRt;
        Texture2D activeReferenceTexture;
        bool doCapture;
        Material m_Material;
        public Material material
        {
            get
            {
                if (m_Material == null)
                    m_Material = new Material(Shader.Find("Hidden/FrameComparison")) { hideFlags = HideFlags.DontSave };
                return m_Material;
            }
        }

        /// ------------------------------------------------------------------------------------
        /// Logic specififc results class

        ResultsData m_TempData; //Dont remove (write result data into this)

        //Structure for results
        [System.Serializable]
        public class ResultsData
        {
            public ResultsDataCommon common; //Dont remove (set automatically)
            public float DiffPercentage;
            public string resultFrame;
            public string comparisonFrame;
        }

        /// ------------------------------------------------------------------------------------
        /// Initial setup methods

        // Set name
        public override void SetName()
        {
            testTypeName = "Frame Comparison";
        }

        //Set model
        public override void SetModel(TestModel inputModel)
        {
            model = (FrameComparisonModel)inputModel;
        }

        //Set results type
        public override void SetResultsType()
        {
            resultsType = typeof(ResultsData);
        }

        /// ------------------------------------------------------------------------------------
        /// Main logic flow methods (overrides) 

        // First injection point for custom code. Runs before any test logic. 
        // - Set up cameras and create RenderTexture
        public override void TestPreProcess()
        {
            temporaryRt = new RenderTexture((int)model.settings.frameResolution.x, (int)model.settings.frameResolution.y, 24);
            if (!SetupCameras())
            {
                Debug.LogWarning("Camera reference missing. Aborting");
            }
            StartTest();
        }

        // Logic for creating baseline data
        public override IEnumerator ProcessBaseline()
        {
            m_TempData = (ResultsData)GetResultsStruct();
            for (int i = 0; i < model.settings.waitFrames; i++)
                yield return new WaitForEndOfFrame();
            model.settings.captureCamera.targetTexture = temporaryRt;
            doCapture = true;
            do { yield return null; } while (m_TempData.resultFrame == null);
            BuildResultsStruct(m_TempData);
        }

        // Logic for creating results data
        public override IEnumerator ProcessResult()
        {
            m_TempData = (ResultsData)GetResultsStruct();
            for (int i = 0; i < model.settings.waitFrames; i++)
                yield return new WaitForEndOfFrame();
            ResultsData referenceData = GenerateBaselineData(ResultsIO.Instance.RetrieveBaseline(testSuiteName, testTypeName, m_TempData.common));
            //ResultsData referenceData = (ResultsData)baseline;
            activeReferenceTexture = Common.BuildTextureFromByteArray(referenceData.common.TestName + "_Reference", referenceData.resultFrame, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode);
            do { yield return null; } while (activeReferenceTexture == null);
            model.settings.captureCamera.targetTexture = temporaryRt;
            doCapture = true;
            do { yield return null; } while (m_TempData.comparisonFrame == null);
            CleanupCameras(); // Need to reset cameras rects here
            if (m_TempData.DiffPercentage < model.settings.passFailThreshold)
                m_TempData.common.PassFail = true;
            else
                m_TempData.common.PassFail = false;
            BuildResultsStruct(m_TempData);
        }

        /// ------------------------------------------------------------------------------------
        /// Custom test logic methods
        /// No relation to base class or any other test type

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
            if (doCapture)
            {
                doCapture = false;
                var rt1 = RenderTexture.GetTemporary((int)model.settings.frameResolution.x, (int)model.settings.frameResolution.y, 24, temporaryRt.format, RenderTextureReadWrite.sRGB);
                Graphics.Blit(temporaryRt, rt1); //Blit camera to the RT
                Texture2D resultsTex = Common.ConvertRenderTextureToTexture2D(activeTestInfo.TestName + "_Result", rt1, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode);
                m_TempData.resultFrame = System.Convert.ToBase64String(resultsTex.EncodeToPNG());
                if (stateType == StateType.CreateResults)
                {
                    var rt2 = RenderTexture.GetTemporary((int)model.settings.frameResolution.x, (int)model.settings.frameResolution.y, 24, temporaryRt.format, RenderTextureReadWrite.sRGB);
                    material.SetTexture("_ReferenceTex", activeReferenceTexture);
                    Graphics.Blit(rt1, rt2, material, 0);
                    Texture2D comparisonTex = Common.ConvertRenderTextureToTexture2D(activeTestInfo.TestName + "_Comparison", rt2, model.settings.frameResolution, model.settings.textureFormat, model.settings.filterMode);
                    m_TempData.DiffPercentage = Common.GetTextureComparisonValue(comparisonTex);
                    m_TempData.comparisonFrame = System.Convert.ToBase64String(comparisonTex.EncodeToPNG());
                    RenderTexture.ReleaseTemporary(rt2);
                    //Destroy(rt2);
                }
                else if (stateType == StateType.CreateBaseline)
                {
                    m_TempData.DiffPercentage = 0.0f;
                    m_TempData.comparisonFrame = "";
                    //Destroy(rt2);
                }
                if (Master.Instance.debugMode == Master.DebugMode.Messages)
                    Debug.Log(this.GetType().Name + " completed blit operations for test " + activeTestInfo.TestName);
                model.settings.captureCamera.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt1); //Release the RT
                temporaryRt.Release();
                //Destroy(rt1);
            }
        }

        //Prepare cameras for capture
        bool SetupCameras()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " is setting up cameras");
            if (dummyCamera == null)
            {
                dummyCamera = this.gameObject.AddComponent<Camera>();
                dummyCamera.rect = new Rect(0, 0, 1, 1);
                //TODO - This still samples a fullscreen blit //dummyCamera.pixelRect = new Rect(0, 0, model.settings.frameResolution.x, model.settings.frameResolution.y);
            }
            if (model.settings.captureCamera)
            {
                model.settings.captureCamera.rect = new Rect(0, 0, 1, 1);
                //TODO - This still samples a fullscreen blit //model.settings.captureCamera.pixelRect = new Rect(0, 0, model.settings.frameResolution.x, model.settings.frameResolution.y);
                return true;
            }
            else
                return false;
        }

        // Cleanup cameras after test finishes
        void CleanupCameras()
        {
            if (dummyCamera)
            {
                Destroy(dummyCamera);
            }
            if (model.settings.captureCamera)
            {
                model.settings.captureCamera.rect = new Rect(0, 0, 1, 1);
            }
        }

        /// ------------------------------------------------------------------------------------
        /// TestViewer related methods
        /// TODO - Revisit this when rewriting the TestViewer

        // Enable and setup the test viewer
        // TODO - Revisit this when rewriting the TestViewer
        public override void EnableTestViewer()
        {
            if (Master.Instance.debugMode == Master.DebugMode.Messages)
                Debug.Log(this.GetType().Name + " enabling Test Viewer");
            object contextObject = new object();
            ResultsData currentResults = (ResultsData)activeResultData;
            switch(stateType)
            {
                case StateType.CreateBaseline:
                    ViewerBarTabData[] tabs = new ViewerBarTabData[2];
                    for(int i = 0; i < tabs.Length; i++)
                        tabs[i] = new ViewerBarTabData();
                    tabs[0].tabName = "Live Camera";
                    tabs[0].tabType = ViewerBarTabType.Camera;
                    tabs[0].tabCamera = model.settings.captureCamera;
                    tabs[1].tabName = "Results Texture";
                    tabs[1].tabType = ViewerBarTabType.Texture;
                    tabs[1].tabTexture = currentResults.resultFrame;
                    tabs[1].textureResolution = model.settings.frameResolution;
                    contextObject = tabs;
                    break;
                case StateType.CreateResults:
                    ViewerBarTabData[] tabs2 = new ViewerBarTabData[4];
                    for(int i = 0; i < tabs2.Length; i++)
                        tabs2[i] = new ViewerBarTabData();
                    tabs2[0].tabName = "Live Camera";
                    tabs2[0].tabType = ViewerBarTabType.Camera;
                    tabs2[0].tabCamera = model.settings.captureCamera;
                    tabs2[1].tabName = "Results Texture";
                    tabs2[1].tabType = ViewerBarTabType.Texture;
                    tabs2[1].tabTexture = currentResults.resultFrame;
                    tabs2[1].textureResolution = model.settings.frameResolution;
                    tabs2[2].tabName = "Comparison Texture";
                    tabs2[2].tabType = ViewerBarTabType.Texture;
                    tabs2[2].tabTexture = currentResults.comparisonFrame;
                    tabs2[2].textureResolution = model.settings.frameResolution;
                    tabs2[3].tabName = "Baseline Texture";
                    tabs2[3].tabType = ViewerBarTabType.Texture;
                    tabs2[3].tabTexture = GenerateBaselineData(ResultsIO.Instance.RetrieveBaseline(testSuiteName, testTypeName, currentResults.common)).resultFrame;
                    tabs2[3].textureResolution = model.settings.frameResolution;
                    contextObject = tabs2;
                    break;   
            }
            ProgressScreen.Instance.SetState(false, ProgressType.LocalSave, ""); // TODO - Move this so its abstracted
            TestViewer.Instance.SetTestViewerState(1, ViewerType.DefaultTabs, contextObject);
        }

        /// ------------------------------------------------------------------------------------
        /// METHODS BELOW ARE NOT CONTEXT SENSITIVE
        /// DO NOT EDIT

        /// ------------------------------------------------------------------------------------
        /// Results
        /// TODO - Attempt to move even more stuff from from here to the abstract class

        // Setup the results structs every test
        public override void SetupResultsStructs()
        {
            ResultsData newResultsData = new ResultsData();
            newResultsData.common = Common.GetCommonResultsData();
            newResultsData.common.SceneName = activeTestInfo.SceneName;
            newResultsData.common.TestName = activeTestInfo.TestName;
            activeResultData = newResultsData;
        }

        // Deserialize ResultsIOData(string arrays) to ResultsData(class)
        ResultsData GenerateBaselineData(ResultsIOData resultsIOData)
        {
            ResultsData resultData = new ResultsData(); //blank results data
            resultData.common = new ResultsDataCommon(); //blank common data

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            FieldInfo[] commonFields = typeof(ResultsDataCommon).GetFields(bindingFlags);
            FieldInfo[] customFields = typeof(ResultsData).GetFields(bindingFlags);

            List<string> commonDataRaw = resultsIOData.resultsRow[0].resultsColumn.GetRange(0, commonFields.Length * 2);
            List<string> resultsDataRaw = resultsIOData.resultsRow[0].resultsColumn.GetRange(commonFields.Length * 2, resultsIOData.resultsRow[0].resultsColumn.Count - (commonFields.Length * 2));

            for (int f = 0; f < customFields.Length; f++)
            {
                if (f == 0)
                {
                    //do the common class
                    for (int cf = 0; cf < commonFields.Length; cf++)
                    {
                        string value = commonDataRaw[(cf * 2) + 1];
                        FieldInfo fieldInfo = resultData.common.GetType().GetField(commonFields[cf].Name);
                        fieldInfo.SetValue(resultData.common, Convert.ChangeType(value, fieldInfo.FieldType));
                    }
                }
                else
                {
                    var value = resultsDataRaw[(f * 2) - 1];
                    FieldInfo fieldInfo = resultData.GetType().GetField(customFields[f].Name);
                    if (fieldInfo.FieldType.IsArray) // This handles arrays
                    {
                        Type type = resultData.GetType().GetField(customFields[f].Name).FieldType.GetElementType();
                        GenerateGenericArray(fieldInfo, resultData.GetType(), resultData, type, value);
                    }
                    else // Non array types
                    {
                        fieldInfo.SetValue(resultData, Convert.ChangeType(value, fieldInfo.FieldType));
                    }
                }
            }
            return resultData;
        }
    }
}
