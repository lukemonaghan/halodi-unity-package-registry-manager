//Modifications copyright (C) 2021 Luke Monaghan

using System;
using Halodi.PackageRegistry.Core;
using Halodi.PackageRegistry.NPM;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    internal class TokenMethod : GUIContent
    {
        internal delegate bool GetToken(ScopedRegistry registry, string username, string password);
        internal string usernameName;
        internal string passwordName;
        internal GetToken action;

        public TokenMethod(string name, string usernameName, string passwordName, GetToken action) : base(name)
        {
            this.usernameName = usernameName;
            this.passwordName = passwordName;
            this.action = action;
        }
    }

    internal struct GetTokenView
    {
        private static TokenMethod[] methods = {
            new TokenMethod("npm login", "username", "password", GetNPMLoginToken),
            new TokenMethod("bintray", "username", "API key", GetBintrayToken),
            // TODO adjust TokenMethod to allow for opening GitHub token URL: https://github.com/settings/tokens/new
        };

        private string username;
        private string password;
        private TokenMethod tokenMethod;
        private int selectedIndex;

        private static string error;
        private static bool GetNPMLoginToken(ScopedRegistry registry, string username, string password)
        {
            error = null;
            NPMResponse response = NPMLogin.GetLoginToken(registry.url, username, password);

            if (string.IsNullOrEmpty(response.ok))
            {
                // EditorUtility.DisplayDialog("Cannot get token", response.error, "Ok");
                error = "Cannot get token: " + response.error;
                return false;
            }
            else
            {
                registry.token = response.token;
                return true;
            }
        }

        private static bool GetBintrayToken(ScopedRegistry registry, string username, string password)
        {
            error = null;
            registry.token = NPMLogin.GetBintrayToken(username, password);
            return !string.IsNullOrEmpty(registry.token);
        }

        internal void DrawTokenView(ScopedRegistry registry)
        {
            EditorGUILayout.LabelField("Generate token", EditorStyles.whiteLargeLabel);
            registry.token = EditorGUILayout.TextField("Token", registry.token);
            
            selectedIndex = EditorGUILayout.Popup(new GUIContent("Method"), selectedIndex, methods);

            var tokenMethod = methods[selectedIndex];

            EditorGUILayout.LabelField(tokenMethod, EditorStyles.whiteLargeLabel);
            username = EditorGUILayout.TextField(tokenMethod.usernameName, username);
            password = EditorGUILayout.PasswordField(tokenMethod.passwordName, password);

            if (GUILayout.Button("Login") && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                if(tokenMethod.action(registry, username, password))
                {
                    // done
                }
            }

            if (!string.IsNullOrEmpty(error))
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }
        }
    }
}
