using UnityEngine;
using UnityEngine.UI;

namespace BrainFailProductions.PolyFew.AsImpL
{
    /// <summary>
    /// UI controller for <see cref="ObjectImporter"/>
    /// </summary>
    [RequireComponent(typeof(ObjectImporter))]
    public class ObjectImporterUI : MonoBehaviour
    {
        [Tooltip("Text for activity messages")]
        public Text progressText;

        [Tooltip("Slider for the overall progress")]
        public Slider progressSlider;

        [Tooltip("Panel with the Image Type set to Filled")]
        public Image progressImage;

        private ObjectImporter objImporter;


        private void Awake()
        {
            if (progressSlider != null)
            {
                progressSlider.maxValue = 100f;
                progressSlider.gameObject.SetActive(false);
            }
            if (progressImage != null)
            {
                progressImage.gameObject.SetActive(false);
            }
            if (progressText != null)
            {
                progressText.gameObject.SetActive(false);
            }
            objImporter = GetComponent<ObjectImporter>();
            // TODO: check and warn
        }


        private void OnEnable()
        {
            objImporter.ImportingComplete += OnImportComplete;
            objImporter.ImportingStart += OnImportStart;
        }


        private void OnDisable()
        {
            objImporter.ImportingComplete -= OnImportComplete;
            objImporter.ImportingStart -= OnImportStart;
        }


        private void Update()
        {
            bool loading = Loader.totalProgress.singleProgress.Count > 0;
            if (!loading) return;
            int numTotalImports = objImporter.NumImportRequests;
            int numImportCompleted = numTotalImports - Loader.totalProgress.singleProgress.Count;

            if (loading)
            {
                float progress = 100.0f * numImportCompleted / numTotalImports;
                float maxSubProgress = 0.0f;
                foreach (SingleLoadingProgress progr in Loader.totalProgress.singleProgress)
                {
                    if (maxSubProgress < progr.percentage) maxSubProgress = progr.percentage;
                }
                progress += maxSubProgress / numTotalImports;
                if (progressSlider != null)
                {
                    progressSlider.value = progress;
                    progressSlider.gameObject.SetActive(loading);
                }
                if (progressImage != null)
                {
                    progressImage.fillAmount = progress / 100f;
                    progressImage.gameObject.SetActive(loading);
                }
                if (progressText != null)
                {
                    if (loading)
                    {
                        progressText.gameObject.SetActive(loading);
                        progressText.text = "Loading " + Loader.totalProgress.singleProgress.Count + " objects...";
                        string loadersText = "";
                        int count = 0;
                        foreach (SingleLoadingProgress i in Loader.totalProgress.singleProgress)
                        {
                            if (count > 4) // maximum 4 messages
                            {
                                loadersText += "...";
                                break;
                            }
                            if (!string.IsNullOrEmpty(i.message))
                            {
                                if (count > 0)
                                {
                                    loadersText += "; ";
                                }
                                loadersText += i.message;
                                count++;
                            }
                        }
                        if (loadersText != "")
                        {
                            progressText.text += "\n" + loadersText;
                        }
                    }
                    else
                    {
                        progressText.gameObject.SetActive(false);
                        progressText.text = "";
                    }
                }
            }
            else
            {
                OnImportComplete();
            }
        }


        private void OnImportStart()
        {
            if (progressText != null)
            {
                progressText.text = "";
            }
            if (progressSlider != null)
            {
                progressSlider.value = 0.0f;
                progressSlider.gameObject.SetActive(true);
            }
            if (progressImage != null)
            {
                progressImage.fillAmount = 0;
                progressImage.gameObject.SetActive(true);
            }
        }


        private void OnImportComplete()
        {
            if (progressText != null)
            {
                progressText.text = "";
            }
            if (progressSlider != null)
            {
                progressSlider.value = 100.0f;
                progressSlider.gameObject.SetActive(false);
            }
            if (progressImage != null)
            {
                progressImage.fillAmount = 1f;
                progressImage.gameObject.SetActive(false);
            }
        }

    }
}
