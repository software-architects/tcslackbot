﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TCSlackbot.Logic.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class BotResponses {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal BotResponses() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TCSlackbot.Logic.Resources.BotResponses", typeof(BotResponses).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are already logged in..
        /// </summary>
        public static string AlreadyLoggedIn {
            get {
                return ResourceManager.GetString("AlreadyLoggedIn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specified object not found..
        /// </summary>
        public static string FilterObjectNotFound {
            get {
                return ResourceManager.GetString("FilterObjectNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are already on break. Did you forget to unpause?.
        /// </summary>
        public static string AlreadyOnBreak {
            get {
                return ResourceManager.GetString("AlreadyOnBreak", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have to login before you can use this bot!\nType login or link to get the login link..
        /// </summary>
        public static string HaveToLogin {
            get {
                return ResourceManager.GetString("HaveToLogin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are not on break. Did you forget to pause?.
        /// </summary>
        public static string NotOnBreak {
            get {
                return ResourceManager.GetString("NotOnBreak", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are not working at the moment. Did you forget to type start?.
        /// </summary>
        public static string NotWorking {
            get {
                return ResourceManager.GetString("NotWorking", resourceCulture);
            }
        }
    }
}
