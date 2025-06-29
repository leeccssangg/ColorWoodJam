﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Tabtale.TTPlugins;
using Tabtale.TTPlugins.UnityIAPWrapper;
using UnityEngine.Scripting;
using UnityEngine.Networking;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

#if UIAP_INSTALLED
using  SubscriptionInfo = UnityEngine.Purchasing.SubscriptionInfo;
#endif

namespace Tabtale.TTPlugins
{
    [System.Serializable]
    public class OnValidateResponse
    {
        public string price = null;
        public string currency = null;
        public string productId = null;
        public bool valid = false;
        public ValidationFailureReason failureReason;
        public string error = "NA";
    }

    public enum ValidationFailureReason
    {
        NOT_ACTIVE, 
        STORE_MISMATCH,
        PRODUCT_ID_MISSING,
        PURCHASE_TOKEN_MISSING,
        CANT_ENDCODE_TOKEN,
        SERVER_ERROR,
        SERVER_CONNECTION_ERROR,
        INTERNAL_ERROR,
        UNKOWN_ERROR,
        NONE
    }
    /// <summary>
    /// This class provides billing functions like purchasing items, restoring purchases.
    /// Also it provides information about purchases and monitoring purchased item status
    /// </summary>
    public class TTPBilling
    {
        /// <summary>
        /// Billing service init event
        /// </summary>
        public static event System.Action<BillerErrors> OnBillingInitEvent;
        /// <summary>
        /// Item purchased event
        /// </summary>
        public static event System.Action<PurchaseIAPResult> OnItemPurchasedEvent;
        /// <summary>
        /// Purchases restored status event
        /// </summary>
        public static event System.Action<bool> OnPurchasesRestoredEvent;
        /// <summary>
        /// Validation purchase event
        /// </summary>
        public static event System.Action<OnValidateResponse> OnValidPurchaseResponseEvent;

        private static System.Action<PurchaseIAPResult> _onPurchasedAction;
        private static Dictionary<string,object> _configuration;
        private static TTPUnityBillingAgent _unityBillingAgent;
        private static List<Action> _noAdsPurchasedListeners;
        private static bool _noAdsPurchased;
        /// <summary>
        /// Init billing service by creating billing agent
        /// </summary>
        /// <param name="onBillingInitAction">Callback for result of initialization</param>
        public static void InitBilling(System.Action<BillerErrors> onBillingInitAction)
        {
            _noAdsPurchasedListeners = new List<Action>();
            if(_unityBillingAgent == null) {
                _unityBillingAgent = new TTPUnityBillingAgent(
                (PurchaseIAPResult purchaseIAPResult) =>
                {
#if UNITY_IOS && !TTP_DEV_MODE && TTP_POPUPMGR
                    TTPPopupMgr.NotifyPurchaseHasFinished();
#endif
                    if (_onPurchasedAction != null)
                        _onPurchasedAction.Invoke(purchaseIAPResult);
                    if (OnItemPurchasedEvent != null)
                        OnItemPurchasedEvent(purchaseIAPResult);

#if UNITY_IOS && !TTP_DEV_MODE
                    if (Impl is IosImpl)
                    {
                        if(purchaseIAPResult.result == PurchaseIAPResultCode.Success)
                            ((IosImpl)Impl).ReportPurchaseToConversion(purchaseIAPResult.purchasedItem.currency, purchaseIAPResult.purchasedItem.localizedPrice, purchaseIAPResult.purchasedItem.mainId, IsConsumable(purchaseIAPResult.purchasedItem.mainId));
                        else
                            Debug.Log("TTPBilling:: purchase failed or cancelled. Will not notify to conversion.");
                    }
#endif
                },  
                (string receipt, float price, string currency, string productId, string transactionId, string originalTransactionId, bool? isFreeTrial) => 
                {
                    Debug.Log("TTPBilling::ValidatePurchaseAction:");
                    TTPPurchaseValidation.SendPurchaseInformation(receipt,
                        price,
                        currency, 
                        productId,
                        _unityBillingAgent.GetIAPType(productId), 
                        Impl.GetPurchaseValidationParams(),
                        transactionId,
                        originalTransactionId,
                        isFreeTrial,
                        OnValidPurchaseResponseEvent,
                        (bool success) =>
                        {
                            Debug.Log("TTPBilling::onValidationFinished isSuccess = " + success);
                            if (success)
                            {
                                var purchaseInfo = new Dictionary<string, object>();
                                purchaseInfo["price"] = price;
                                purchaseInfo["currency"] = currency;
                                purchaseInfo["productId"] = productId;
                                TTPCore.WriteEventToEventsFileHandler("unityTransaction", "ExportEvent", TTPJson.Serialize(purchaseInfo));
                                Impl.SetPurchaserKeywordForRequest();
                                TTPCore.ReportPurchase(price, IsNoAdsItem(productId) , currency, (convertedPrice, error) =>
                                {
                                    if (error == null)
                                    {
                                        Debug.Log($"TTPBilling::onValidationFinished stored. Price ${price}${currency}, converted: ${convertedPrice}USD");
                                    }
                                    else
                                    {
                                        Debug.Log($"TTPBilling::onValidationFinished failed to store: ${error}");
                                    }
                                });
                            }
                        });
                },
                NoAdsPurchased,
                (bool success) =>
                {
                    if (OnPurchasesRestoredEvent != null)
                        OnPurchasesRestoredEvent.Invoke(success);
                },
                Configuration);

                _unityBillingAgent.InitBilling((BillerErrors errors) => {
                    if (OnBillingInitEvent != null)
                        OnBillingInitEvent(errors);
                    if (onBillingInitAction != null)
                        onBillingInitAction.Invoke(errors);
                });
            }
            else
            {
                Debug.LogWarning("TTPBilling::InitBilling: called already. Will not init again");
            }
        }

        /// <summary>
        /// Purchase item by ID
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <param name="onPurchasedAction">Callback for result of purchasing</param>
        public static void PurchaseItem(string itemId, System.Action<PurchaseIAPResult> onPurchasedAction)
        {
            if(itemId == TestIAP.NO_ADS_ITEM_ID && TestIAP.IsTest())
            {
                bool isPurchased = TestIAP.ProccessNoAdsPurchase(itemId, onPurchasedAction);
                NoAdsPurchased(isPurchased);
                return;
            }
            if (_unityBillingAgent != null)
            {
                _onPurchasedAction = onPurchasedAction;
                _unityBillingAgent.PurchaseItem(itemId, () =>
                {
                    Debug.Log("TTPBilling::Purchase has started");
#if UNITY_IOS && !TTP_DEV_MODE && TTP_POPUPMGR
                    TTPPopupMgr.NotifyPurchaseHasStarted();
#endif
                });
            }
            else
            {
                Debug.LogError("TTPBilling::PurchaseItem called but billing was not initiated.");
            }
        }
        /// <summary>
        /// Restore purchased items
        /// </summary>
        /// <param name="onPurchasesRestoredAction">Callback for result of restoring</param>
        public static void RestorePurchases(System.Action<bool> onPurchasesRestoredAction)
        {
            if (_unityBillingAgent != null)
            {
                _unityBillingAgent.RestorePurchases((bool success) =>
                {
                    if (onPurchasesRestoredAction != null)
                        onPurchasesRestoredAction.Invoke(success);
                    if (OnPurchasesRestoredEvent != null)
                        OnPurchasesRestoredEvent(success);
                });
            }
            else
            {
                Debug.LogError("TTPBilling::RestorePurchases called but billing was not initiated.");
            }
        }
        /// <summary>
        /// Define purchased status of the item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>True - if item is purchased</returns>
        public static bool IsPurchased(string itemId)
        {
#if UNITY_IOS && !UNITY_EDITOR && !TTP_DEV_MODE
            Debug.Log("TTPBilling::isPurchased called" + itemId);
            if (Impl != null && !IsNoAdsItem(itemId)){
                    IosImpl iOSImpl = (IosImpl)Impl;
                    if(iOSImpl.WasUserDetectedInChina()){
                        Debug.Log("TTPBilling::user was detected in China.");
                        return true;
                    }
                }
#endif

            if (_unityBillingAgent != null)
            {
                return _unityBillingAgent.IsPurchased(itemId);
            }
            else
            {
                Debug.LogError("TTPBilling::IsPurchased called but billing was not initiated.");
                return false;
            }
        }
        /// <summary>
        /// Define is item consumable or not
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns>True - if item is consumable</returns>
        public static bool IsConsumable(string itemId)
        {
            if (_unityBillingAgent != null)
            {
                return _unityBillingAgent.IsConsumable(itemId);
            }
            else
            {
                Debug.LogError("TTPBilling::IsConsumable called but billing was not initiated.");
                return false;
            }
        }
        /// <summary>
        /// Define that item is noAds 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns>True - if it's noAds item</returns>
        public static bool IsNoAdsItem(string itemId)
        {
            if (_unityBillingAgent != null)
            {
                return _unityBillingAgent.IsNoAdsItem(itemId);
            }
            else
            {
                Debug.LogError("TTPBilling::IsNoAdsItem called but billing was not initiated.");
                return false;
            }
        }
        /// <summary>
        /// Returns localized price for the item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>String with localized price</returns>
        public static string GetLocalizedPriceString(string itemId)
        {
            if (_unityBillingAgent != null)
            {
                return _unityBillingAgent.GetLocalizedPriceString(itemId);
            }
            else
            {
                Debug.LogError("TTPBilling::GetLocalizedPriceString called but billing was not initiated.");
                return null;
            }
        }
        /// <summary>
        /// Ruturns currency symbol according to ISO
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>String with currency symbol</returns>
        public static string GetISOCurrencySymbol(string itemId)
        {
            if (_unityBillingAgent != null)
            {
                return _unityBillingAgent.GetISOCurrencySymbol(itemId);
            }
            else
            {
                Debug.LogError("TTPBilling::GetISOCurrencySymbol called but billing was not initiated.");
                return null;
            }
        }
        /// <summary>
        /// Returns price value in local currency 
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>Decimal price value</returns>
        public static decimal GetPriceInLocalCurrency(string itemId)
        {
            if (_unityBillingAgent != null)
            {
                return _unityBillingAgent.GetPriceInLocalCurrency(itemId);
            }
            else
            {
                Debug.LogError("TTPBilling::GetPriceInLocalCurrency called but billing was not initiated.");
                return 0;
            }
        }
        /// <summary>
        /// Remove all transactions
        /// </summary>
        public static void ClearTransactions()
        {
            if (_unityBillingAgent != null)
            {
                _unityBillingAgent.ClearTransactions();
            }
            else
            {
                Debug.LogError("TTPBilling::ClearTransactions called but billing was not initiated.");
            }
        }
        /// <summary>
        /// Returns subscription information about the item
        /// </summary>
        /// <param name="itemId">Item ID</param>
        /// <returns>Struct with subscription information</returns>
        public static SubscriptionInfo GetSubscriptionInfo(string itemId)
        {
            if (_unityBillingAgent != null)
            {
                return _unityBillingAgent.GetSubscriptionInfo(itemId);
            }
            else
            {
                Debug.LogError("TTPBilling::GetSubscriptionInfo called but billing was not initiated.");
                return null;
            }
        }
        [Preserve]
        static void InternalInit()
        {
            Debug.Log("TTPBilling::InternalInit called");
            if (Configuration != null)
            {
                if(!Configuration.ContainsKey("iaps") || ((Configuration["iaps"] as List<object>) == null || (Configuration["iaps"] as List<object>).Count == 0))
                {
                    Debug.Log("TTPBilling::InternalInit: iaps empty. will not initialize billing.");
                    return;
                }

                if (!Configuration.ContainsKey("deferInit") || (Configuration.ContainsKey("deferInit") && Configuration["deferInit"] is bool && !(bool)Configuration["deferInit"]))
                {
                    Debug.Log("TTPBilling::InternalInit: deferInit false. initializing billing.");
                    InitBilling(null);
                }
            }
        }
    
        static void NoAdsPurchased(bool purchased)
        {
            Debug.Log("TTPBilling::NoAdsPurchased:purchased=" + purchased);
            _noAdsPurchased = purchased;
            if (_noAdsPurchased && _noAdsPurchasedListeners.Count > 0)
            {
                foreach (var action in _noAdsPurchasedListeners)
                {
                    action.Invoke();
                }
            }
            if (Impl != null)
                Impl.NotifyNoAdsPurchased(purchased);
        }

        public static bool IsNoAdsPurchased()
        {
            return _noAdsPurchased;
        }

        static void RegisterNoAdsPurchasedListener(Action listener)
        {
            if (_noAdsPurchased)
            {
                listener.Invoke();
            }
            _noAdsPurchasedListeners.Add(listener);
        }

        private static Dictionary<string,object> Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    string configurationJson = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetConfigurationJson("billing");
                    if (!string.IsNullOrEmpty(configurationJson))
                    {
                        _configuration = TTPJson.Deserialize(configurationJson) as Dictionary<string,object>;
                    }
                }
                return _configuration;
            }
        }

        private static ITTPBilling _impl;
        private static ITTPBilling Impl
        {
            get
            {
                if (_impl == null)
                {
#if !TTP_BILLING
                    _impl = new EditorImpl();
                    return _impl;
#endif
                    if (TTPCore.DevMode)
                    {
                        _impl = new EditorImpl();
                    }
                    else if (_impl == null)
                    {
                        if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android ||
                            UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer)
                        {
#if UNITY_ANDROID
                            _impl = new AndroidImpl();
#endif
#if UNITY_IOS && !TTP_DEV_MODE
                        _impl = new IosImpl();
#endif

                        }
                        else
                        {
#if UNITY_EDITOR
                            _impl = new EditorImpl();
#endif
                        }
                    }
                    if (_impl == null)
                    {
                        Debug.LogError("TTPBilling::Impl: failed to create native impl");
                    }
                    return _impl;
                }
                return _impl;
            }
        }

        private interface ITTPBilling
        {
            void NotifyNoAdsPurchased(bool purchased);
            Dictionary<string, object> GetPurchaseValidationParams();
            void SetPurchaserKeywordForRequest();
        }

#if UNITY_IOS && !TTP_DEV_MODE
        private class IosImpl : ITTPBilling
        {
            [DllImport("__Internal")]
            private static extern void ttpNoAdsPurchased(bool purchased);

            [DllImport("__Internal")]
            private static extern bool ttpWasUserDetectedInChina();
            
            [DllImport("__Internal")]
            private static extern void ttpReportPurchaseToConversion(string message);

            [DllImport("__Internal")]
            private static extern string ttpGetPurchaseValidationParams();

            [DllImport("__Internal")]
            private static extern void ttpSetPurchaserKeywordForRequest();

            public void NotifyNoAdsPurchased(bool purchased)
            {
                ttpNoAdsPurchased(purchased);
            }

            public bool WasUserDetectedInChina()
            {
                return ttpWasUserDetectedInChina();
            }

            public void ReportPurchaseToConversion(string currency, float price, string productId, bool consumable)
            {
                var consumableStr = consumable ? "true" : "false";
                var message = "{\"currency\":\""+currency+"\",\"price\":"+price.ToString()+",\"productId\":\""+productId+"\",\"consumable\":"+consumableStr+"}";
                ttpReportPurchaseToConversion(message);
            }

            public Dictionary<string, object> GetPurchaseValidationParams()
            {
                var str = ttpGetPurchaseValidationParams();
                if (str != null)
                {
                    var dict = TTPJson.Deserialize(str) as Dictionary<string, object>;
                    if (dict != null)
                    {
                        return dict;
                    }
                }
                return new Dictionary<string, object>();
            }

            public void SetPurchaserKeywordForRequest()
            {
                ttpSetPurchaserKeywordForRequest();
            }
        }
#endif
#if UNITY_ANDROID
        private class AndroidImpl : ITTPBilling
        {
            private const string SERVICE_GET_METHOD = "getBilling";

            private AndroidJavaObject _serivceJavaObject;

            private AndroidJavaObject ServiceJavaObject
            {
                get
                {
                    if (_serivceJavaObject == null)
                    {
                        _serivceJavaObject = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetServiceJavaObject(SERVICE_GET_METHOD);
                    }
                    if (_serivceJavaObject == null)
                        Debug.LogError("TTPBilling::AndroidImpl: failed to get native instance.");
                    return _serivceJavaObject;
                }
            }

            public void NotifyNoAdsPurchased(bool purchased)
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("setNoAdsItemPurchased",new object[] {purchased});
                }
            }

            public Dictionary<string, object> GetPurchaseValidationParams()
            {
                if (ServiceJavaObject != null)
                {
                    string str = ServiceJavaObject.Call<string>("getPurchaseValidationParams");
                    if (str != null)
                    {
                        return TTPJson.Deserialize(str) as Dictionary<string, object>;
                    }
                }
                return new Dictionary<string, object>();
            }

            public void SetPurchaserKeywordForRequest()
            {
                if (ServiceJavaObject != null)
                {
                    ServiceJavaObject.Call("setPurchaserKeywordForRequest");
                }
            }
        }
#endif
        //#if UNITY_EDITOR

        private class EditorImpl : ITTPBilling
        {
            public void NotifyNoAdsPurchased(bool purchased) {}
            public Dictionary<string, object> GetPurchaseValidationParams()
            {
                return new Dictionary<string, object>();
            }

            public void SetPurchaserKeywordForRequest()
            {
                Debug.Log("TTPBilling::SetPurchaserKeywordForRequest:is only a native implementation");
            }
        }
//#endif
    }
}