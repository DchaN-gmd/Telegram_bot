using System;
//using UnityEditor;
//using UnityEngine;
using System.Collections.Generic;

namespace DevXUnity.SerialNumberLicense.Editor
{
#if  UNITY_EDITOR
    public class SerialNumberGeneratorUI : EditorWindow
    {
        #region Static fields
        private static GUIStyle _tableRowStyle;
        private static GUIStyle _tableHeaderStyle;
        private static GUIStyle _tableHeaderStyle2;
        private static GUIStyle _toolBarButtonStyle;
        private static Dictionary<string, Texture2D> _cacheTextures = new();
        #endregion
        
        #region Fields
        private string _email;
        private string _hardwareID;

        private bool _isExpiration;
        private int _expirationDays;
        private string _comment;

        private Vector2 _scrollPos;
        #endregion

        #region Properties
        private static GUIStyle TableRowStyle
        {
            get
            {
                if (_tableRowStyle == null)
                {
                    _tableRowStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        margin = new RectOffset(1, 1, 1, 1),
                        padding = new RectOffset(1, 1, 4, 4)
                    };
                }
                
                return _tableRowStyle;
            }
        }
        
        private static GUIStyle TableHeaderStyle
        {
            get
            {
                if (_tableHeaderStyle != null) return _tableHeaderStyle;
                _tableHeaderStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    margin = new RectOffset(1, 1, 1, 1),
                    padding = new RectOffset(0, 0, 0, 0)
                };
                return _tableHeaderStyle;
            }
        }
        
        private static GUIStyle TableHeaderStyle2
        {
            get
            {
                if (_tableHeaderStyle2 != null) return _tableHeaderStyle2;
                
                _tableHeaderStyle2 = new GUIStyle
                {
                    padding = new RectOffset(1, 1, 4, 4),
                    margin = new RectOffset(1, 1, 1, 1)
                };
                
                {
                    var texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, new Color(0.0f, 0.0f, 0.0f, 0.15f));
                    texture.Apply();

                    _tableHeaderStyle2.normal.background = texture;
                    _tableHeaderStyle2.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                }
                
                {
                    var texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, new Color(0.0f, 0.0f, 0.0f, 0.14f));
                    texture.Apply();

                    _tableHeaderStyle2.hover.background = texture;
                }
                
                return _tableHeaderStyle2;
            }
        }
        
        private static GUIStyle ToolBarButtonStyle
        {
            get
            {
                if (_toolBarButtonStyle != null) return _toolBarButtonStyle;
                _toolBarButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    fixedHeight = 22
                };
                return _toolBarButtonStyle;
            }
        }
        #endregion
        
        [MenuItem("Window/DevXUnityTools-SerialNumbers")]
        internal static void LicenseGeneratorShow()
        {
            var window = (SerialNumberGeneratorUI)GetWindow(typeof(SerialNumberGeneratorUI));
            
            window.titleContent = new GUIContent("SerialNumbers");
            window.minSize = new Vector2(800, 600);
            window.Show(true);
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("DevXUnity - Serial number generator Ver 2.4");

            EditorGUILayout.LabelField("htttp://DevXDevelopment.com");
            EditorGUILayout.LabelField("For additional protection it is necessary to use obfuscator (DevXUnity-ObfuscatorPro/Base)");

            GUILayout.Space(10);

            GUILayout.BeginVertical(TableRowStyle);
            {
                _hardwareID = EditorGUILayout.TextField("HardwareID", _hardwareID);
                _email = EditorGUILayout.TextField("eMail", _email);
                _isExpiration = EditorGUILayout.ToggleLeft("Date restrictions", _isExpiration);
                if (_isExpiration)
                {
                    _expirationDays = EditorGUILayout.IntField("Expires through (days)", _expirationDays);
                    if (_expirationDays < 1) _expirationDays = 1;
                    if (_expirationDays > 365) _expirationDays = 365;
                }
                _comment = EditorGUILayout.TextField("Comment", _comment);
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(new GUIContent("Create serial number", FindTexture("TextWrite16")), ToolBarButtonStyle))
                {
                    if (string.IsNullOrEmpty(_hardwareID) ==false || _isExpiration)
                    {
                        // Make license by HardwareID and save in folder
                        DateTime? expDate = null;
                        if (_isExpiration) expDate = DateTime.UtcNow.AddDays(_expirationDays);

                        SerialNumberGeneratorTools.MakeLicense(_hardwareID, expDate, _comment + "", _email+"");
                        SerialNumberGeneratorTools.LicenseList = null;
                        GUI.FocusControl("");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Warning", "You must specify the 'HardwareID' or (and) 'Date restrictions'", "ok");
                    }
                }

                if (GUILayout.Button(new GUIContent("Clear filter", FindTexture("New16")), ToolBarButtonStyle))
                { 
                    _email = "";
                    _hardwareID = ""; 
                    _comment = ""; 
                    _isExpiration = false; 
                    GUI.FocusControl("");
                }
                
                if (GUILayout.Button(new GUIContent("Update list", FindTexture("Update16")), ToolBarButtonStyle))
                {
                      SerialNumberGeneratorTools.LicenseList = null;
                      GUI.FocusControl("");
                }

                if (GUILayout.Button(new GUIContent("Open license directory", FindTexture("Folder16")), ToolBarButtonStyle))
                {
                    System.Diagnostics.Process.Start(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), SerialNumberGeneratorTools.BasePath));
                }

                if (GUILayout.Button(new GUIContent("Re-generate as base keys", FindTexture("Build16")), ToolBarButtonStyle))
                {
                    // Re-generate simple keys
                    SerialNumberGeneratorTools.UpdateKeys(true); 
                    GUI.FocusControl("");
                    AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
                }
                
                if (GUILayout.Button(new GUIContent("Re-generate as SDA keys", FindTexture("Build16")), ToolBarButtonStyle))
                {
                    // Re-generate DSA keys
                    SerialNumberGeneratorTools.UpdateKeys(true, true);
                    GUI.FocusControl("");
                    AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            
            // Header
            GUILayout.BeginVertical(TableHeaderStyle);
            GUILayout.BeginHorizontal(TableHeaderStyle2);
            {
                GUILayout.Label("Create date", EditorStyles.boldLabel, GUILayout.Width(150));
                GUILayout.Label("Name (HardwareID)", EditorStyles.boldLabel, GUILayout.Width(250));
                GUILayout.Label("Serial number", EditorStyles.boldLabel, GUILayout.Width(200));
                GUILayout.Label("eMail", EditorStyles.boldLabel, GUILayout.Width(150));
                GUILayout.Label("Comment", EditorStyles.boldLabel, GUILayout.Width(200));
                GUILayout.Space(5);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            // Get all license items
            var rows = SerialNumberGeneratorTools.GetLicenseList();
            if (rows.Count == 0)
            {
                GUILayout.BeginVertical(TableRowStyle);
                EditorGUILayout.LabelField("(empty list)");
                GUILayout.EndVertical();
            }
            else
            {
                foreach (var row in rows)
                {
                    if (string.IsNullOrEmpty(_hardwareID) == false)
                    {
                        if (row.Name != null && row.Name.IndexOf(_hardwareID, StringComparison.InvariantCultureIgnoreCase) < 0)
                            continue;
                    }

                    if (string.IsNullOrEmpty(_email) == false)
                    {
                        if (row.Email != null && row.Email.IndexOf(_email, StringComparison.InvariantCultureIgnoreCase) < 0)
                            continue;
                    }

                    // License info row
                    GUILayout.BeginVertical(TableRowStyle);

                    GUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(new GUIContent(row.CreateDate.ToString("yyyy.MM.dd HH:mm:ss"), FindTexture("Activate16")), EditorStyles.label, GUILayout.Width(150));
                    EditorGUILayout.TextArea(row.Name, EditorStyles.label, GUILayout.Width(250));
                    EditorGUILayout.TextArea(row.LicenseContent, GUILayout.Width(200));
                    EditorGUILayout.TextArea(row.Email, EditorStyles.label, GUILayout.Width(150));
                    EditorGUILayout.TextArea(row.Comment, EditorStyles.label, GUILayout.Width(200));

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                }
            }
            GUILayout.EndScrollView();
        }

        private static Texture2D FindTexture(string name)
        {
            _cacheTextures ??= new Dictionary<string, Texture2D>();

            if (_cacheTextures.ContainsKey(name) && _cacheTextures[name] != null)
                return _cacheTextures[name];

            var text = Resources.Load<Texture2D>(name);
            if (text == null)
            {
                _cacheTextures[name] = new Texture2D(1, 1);
                return null;
            }

            _cacheTextures[name] = text;
            return text;
        }
    }
#endif
}
