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
        ///   Looks up a localized string similar to You are already on break. Did you forget to unpause?.
        /// </summary>
        public static string AlreadyOnBreak {
            get {
                return ResourceManager.GetString("AlreadyOnBreak", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You are already working..
        /// </summary>
        public static string AlreadyWorking {
            get {
                return ResourceManager.GetString("AlreadyWorking", resourceCulture);
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
        ///   Looks up a localized string similar to There was one or more parameters missing. .
        /// </summary>
        public static string InvalidParameter {
            get {
                return ResourceManager.GetString("InvalidParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have succesfully logged out. Goodbye..
        /// </summary>
        public static string LogoutMessage {
            get {
                return ResourceManager.GetString("LogoutMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have to login before you can use this bot!
        ///Type login or link to get the login link..
        /// </summary>
        public static string NotLoggedIn {
            get {
                return ResourceManager.GetString("NotLoggedIn", resourceCulture);
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
        
        /// <summary>
        ///   Looks up a localized string similar to Break has been set. You can now relax..
        /// </summary>
        public static string StartedBreak {
            get {
                return ResourceManager.GetString("StartedBreak", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You started working..
        /// </summary>
        public static string StartedWorking {
            get {
                return ResourceManager.GetString("StartedWorking", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You stopped working..
        /// </summary>
        public static string StoppedWorking {
            get {
                return ResourceManager.GetString("StoppedWorking", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hey, you have been working for a long time now. Why don&apos;t you take a break?.
        /// </summary>
        public static string TakeABreak {
            get {
                return ResourceManager.GetString("TakeABreak", resourceCulture);
            }
        }
    }
}
