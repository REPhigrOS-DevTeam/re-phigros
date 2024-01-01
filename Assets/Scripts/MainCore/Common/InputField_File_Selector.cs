using System.Linq;
using MainCore.UI.Utils;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace MainCore.Common
{
    public class InputField_File_Selector : MonoBehaviour
    {
        [SerializeField] private Button browseButton;
        [SerializeField] private bool enableAllFileSelector;
        [SerializeField] private string[] fileExtensions;
        [SerializeField] private FileBrowser.PickMode pickMode;
        private InputField_String_Setting inputFieldStringSetting;
        public InputField_String_Setting BaseData => inputFieldStringSetting;
        private bool locked;

        private void Awake()
        {
            inputFieldStringSetting = gameObject.GetComponent<InputField_String_Setting>();
            browseButton.onClick.AddListener(Browse);
        }

        private void Browse()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            if (pickMode != FileBrowser.PickMode.FilesAndFolders)
            {
                if (pickMode == FileBrowser.PickMode.Folders)
                {
                    OpenFile.LoadFolder(path => inputFieldStringSetting.SetValue(path),
                        () => { }, Application.persistentDataPath, "选择...", "确定");
                }
                else
                {
                    OpenFile.LoadFile(path => inputFieldStringSetting.SetValue(path),
                        () => { }, string.Join("|", fileExtensions.Select(s => $"|*{s}")), Application.persistentDataPath, "选择...", "确定");
                }
                return;
            }
#endif
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

        public void Lock()
        {
            if (locked) return;
            locked = true;
            Destroy(inputFieldStringSetting);
            browseButton.interactable = false;
            InputField inputField = gameObject.GetComponent<InputField>().GetComponent<InputField>();
            inputField.interactable = false;
            inputField.text = "已禁用";
        }
    }
}