using System;
using SimpleFileBrowser;
using UnityEngine;

namespace MainCore.Common
{
    [RequireComponent(typeof(InputField_String_Setting))]
    public class InputField_File_Selector : MonoBehaviour
    {
        public bool enableAllFileSelector;
        public string[] fileExtensions;
        public FileBrowser.PickMode pickMode;
        private InputField_String_Setting inputFieldStringSetting;

        private void Awake()
        {
            inputFieldStringSetting = gameObject.GetComponent<InputField_String_Setting>();
        }

        public void Select()
        {
            if (fileExtensions.Length > 0 && pickMode == FileBrowser.PickMode.Files)
            {
                FileBrowser.SetFilters(enableAllFileSelector, fileExtensions);
            }
            else
            {
                FileBrowser.SetFilters(pickMode == FileBrowser.PickMode.Files);
            }

            FileBrowser.ShowLoadDialog(paths => inputFieldStringSetting.SetValue(paths[0]),
                () => { }, pickMode, false, Application.persistentDataPath, "", "选择...", "确定");
        }
    }
}