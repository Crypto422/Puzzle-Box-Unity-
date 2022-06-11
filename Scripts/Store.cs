using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace Assets.Scripts.Utils
{
	internal class Store : IStoreListener
	{
		[Serializable]
		private sealed class __c
		{
			public static readonly Store.__c __9 = new Store.__c();

			public static Action<bool> __9__27_0;

			internal void _RestorePurchase_b__27_0(bool result)
			{
			}
		}

		private IAppleExtensions m_AppleExtensions;

		private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;

		private static Store instance;

		private IStoreController controller;

		private IExtensionProvider extensions;

		private int errorno = -2;

		public ProductCatalog catalog;

		public Action<Product, bool> ProcessNonConsumableHandle;

		public Action<SubscriptionInfo, bool> ProcessSubscriptionHandle;

		public Action<Product, bool> ProcessConsumableHanle;

		public IStoreController Controller
		{
			get
			{
				return this.controller;
			}
			set
			{
				this.controller = value;
			}
		}

		public IExtensionProvider Extensions
		{
			get
			{
				return this.extensions;
			}
			set
			{
				this.extensions = value;
			}
		}

		public int Errorno
		{
			get
			{
				return this.errorno;
			}
			set
			{
				this.errorno = value;
			}
		}

		public static Store getInstance()
		{
			if (Store.instance == null)
			{
				Store.instance = new Store();
			}
			return Store.instance;
		}

		public Store()
		{
			this.catalog = ProductCatalog.LoadDefaultCatalog();
		}

		public void Init()
		{
			ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(), Array.Empty<IPurchasingModule>());
			IAPConfigurationHelper.PopulateConfigurationBuilder(ref configurationBuilder, this.catalog);
			if (Application.platform == RuntimePlatform.Android)
			{
				configurationBuilder.Configure<IGooglePlayConfiguration>().SetPublicKey("MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApXdW5CQVgmaP+nnBb57o/J8dvPkHsw+GptTDbNWXXnSPTsebfwCIDW76euFqaNTExwHnN8vLOH1KWGmfOfvhbxZKoXrEet4nw7f/0jYj9EE6I1ctZ5w+liT6IpBm+RXb8Ubdl6HxWjUacMP5jK4nvSkl0nziH2Aoy2eGyGXmiqFsgv4zmY59zkbpwUqrMMEVQAkPvY6s2hM5E4P3E3vGIb+LRPr2wzBXQKfMPpX1qoXR/88kjcNpWdJe38Epl9/qpzJA11J1wUXgG1FmMGTUZuuP5pZP7cvRuKDt/cvnabp7iNKqZfa8hTAeawA02MTHMnNh40jQBxuBPW2V4dX8/QIDAQAB");
			}
			UnityPurchasing.Initialize(this, configurationBuilder);
		}

		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			this.Controller = controller;
			this.Extensions = extensions;
			this.Errorno = -1;
			this.m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
			this.m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
			CrossPlatformValidator crossPlatformValidator = new CrossPlatformValidator(null, null, Application.identifier);
			Product[] all = controller.products.all;
			for (int i = 0; i < all.Length; i++)
			{
				Product product = all[i];
				if (product.availableToPurchase && product.hasReceipt)
				{
					switch (product.definition.type)
					{
					case ProductType.Consumable:
						goto IL_15A;
					case ProductType.NonConsumable:
						try
						{
							IPurchaseReceipt[] array = crossPlatformValidator.Validate(product.receipt);
							for (int j = 0; j < array.Length; j++)
							{
								IPurchaseReceipt arg_AD_0 = array[j];
								Action<Product, bool> expr_B4 = this.ProcessNonConsumableHandle;
								if (expr_B4 != null)
								{
									expr_B4(product, true);
								}
							}
							goto IL_15A;
						}
						catch (IAPSecurityException)
						{
							UnityEngine.Debug.Log("validate fail");
							goto IL_15A;
						}
						break;
					case ProductType.Subscription:
						break;
					default:
						goto IL_15A;
					}
					if (this.checkIfProductIsAvailableForSubscriptionManager(product.receipt))
					{
						Dictionary<string, string> introductoryPriceDictionary = this.m_AppleExtensions.GetIntroductoryPriceDictionary();
						string intro_json = (introductoryPriceDictionary == null || !introductoryPriceDictionary.ContainsKey(product.definition.storeSpecificId)) ? null : introductoryPriceDictionary[product.definition.storeSpecificId];
						SubscriptionInfo subscriptionInfo = new SubscriptionManager(product, intro_json).getSubscriptionInfo();
						Action<SubscriptionInfo, bool> expr_140 = this.ProcessSubscriptionHandle;
						if (expr_140 != null)
						{
							expr_140(subscriptionInfo, true);
						}
					}
					else
					{
						UnityEngine.Debug.Log("This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
					}
				}
				IL_15A:;
			}
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			this.Errorno = (int)error;
		}

		public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
		{
			UnityEngine.Debug.Log("on purchase failed");
		}

		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
		{
			RuntimePlatform platform = Application.platform;
			if (platform != RuntimePlatform.OSXEditor)
			{
				switch (platform)
				{
				case RuntimePlatform.WindowsEditor:
					break;
				case RuntimePlatform.IPhonePlayer:
				case RuntimePlatform.Android:
				{
					switch (e.purchasedProduct.definition.type)
					{
					case ProductType.Consumable:
					{
						CrossPlatformValidator crossPlatformValidator = new CrossPlatformValidator(null, null, Application.identifier);
						try
						{
							crossPlatformValidator.Validate(e.purchasedProduct.receipt);
							Action<Product, bool> expr_82 = this.ProcessConsumableHanle;
							if (expr_82 != null)
							{
								expr_82(e.purchasedProduct, false);
							}
							goto IL_18A;
						}
						catch (IAPSecurityException)
						{
							UnityEngine.Debug.Log("Invalid receipt, not unlocking content");
							goto IL_18A;
						}
						break;
					}
					case ProductType.NonConsumable:
						break;
					case ProductType.Subscription:
						goto IL_FD;
					default:
						goto IL_18A;
					}
					CrossPlatformValidator crossPlatformValidator2 = new CrossPlatformValidator(null, null, Application.identifier);
					try
					{
						crossPlatformValidator2.Validate(e.purchasedProduct.receipt);
						Action<Product, bool> expr_D6 = this.ProcessNonConsumableHandle;
						if (expr_D6 != null)
						{
							expr_D6(e.purchasedProduct, false);
						}
						goto IL_18A;
					}
					catch (IAPSecurityException)
					{
						UnityEngine.Debug.Log("Invalid receipt, not unlocking content");
						goto IL_18A;
					}
					IL_FD:
					if (this.checkIfProductIsAvailableForSubscriptionManager(e.purchasedProduct.receipt))
					{
						Dictionary<string, string> introductoryPriceDictionary = this.m_AppleExtensions.GetIntroductoryPriceDictionary();
						string intro_json = (introductoryPriceDictionary == null || !introductoryPriceDictionary.ContainsKey(e.purchasedProduct.definition.storeSpecificId)) ? null : introductoryPriceDictionary[e.purchasedProduct.definition.storeSpecificId];
						SubscriptionInfo subscriptionInfo = new SubscriptionManager(e.purchasedProduct, intro_json).getSubscriptionInfo();
						Action<SubscriptionInfo, bool> expr_170 = this.ProcessSubscriptionHandle;
						if (expr_170 != null)
						{
							expr_170(subscriptionInfo, false);
						}
					}
					else
					{
						UnityEngine.Debug.Log("This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class.");
					}
					IL_18A:
					if (PlayerPrefs.GetInt(e.purchasedProduct.transactionID, 0) == 0)
					{
						Product purchasedProduct = e.purchasedProduct;
						//AppsflyerUtils.TrackPurchase(purchasedProduct.definition.id, (float)purchasedProduct.metadata.localizedPrice, purchasedProduct.metadata.isoCurrencyCode);
						PlayerPrefs.GetInt(e.purchasedProduct.transactionID, 1);
						return PurchaseProcessingResult.Complete;
					}
					return PurchaseProcessingResult.Complete;
				}
				case RuntimePlatform.PS3:
				case RuntimePlatform.XBOX360:
					return PurchaseProcessingResult.Complete;
				default:
					return PurchaseProcessingResult.Complete;
				}
			}
			switch (e.purchasedProduct.definition.type)
			{
			case ProductType.Consumable:
			{
				Action<Product, bool> expr_219 = this.ProcessConsumableHanle;
				if (expr_219 != null)
				{
					expr_219(e.purchasedProduct, false);
				}
				break;
			}
			case ProductType.NonConsumable:
			{
				Action<Product, bool> expr_233 = this.ProcessNonConsumableHandle;
				if (expr_233 != null)
				{
					expr_233(e.purchasedProduct, false);
				}
				break;
			}
			case ProductType.Subscription:
			{
				Action<SubscriptionInfo, bool> expr_24D = this.ProcessSubscriptionHandle;
				if (expr_24D != null)
				{
					expr_24D(new SubscriptionInfo(e.purchasedProduct.definition.id), false);
				}
				break;
			}
			}
			return PurchaseProcessingResult.Complete;
		}

		public void InitiatePurchase(string productId)
		{
			if (!this.IsInitialized(true))
			{
				return;
			}
			this.Controller.InitiatePurchase(productId);
		}

		public void RestorePurchase()
		{
			RuntimePlatform platform = Application.platform;
			if (platform <= RuntimePlatform.IPhonePlayer)
			{
				if (platform != RuntimePlatform.OSXPlayer && platform != RuntimePlatform.IPhonePlayer)
				{
					return;
				}
			}
			else if (platform == RuntimePlatform.Android || platform != RuntimePlatform.tvOS)
			{
				return;
			}
			IAppleExtensions arg_47_0 = this.Extensions.GetExtension<IAppleExtensions>();
			Action<bool> arg_47_1;
			if ((arg_47_1 = Store.__c.__9__27_0) == null)
			{
				arg_47_1 = (Store.__c.__9__27_0 = new Action<bool>(Store.__c.__9._RestorePurchase_b__27_0));
			}
			arg_47_0.RestoreTransactions(arg_47_1);
		}

		private bool IsInitialized(bool isToast = false)
		{
			if (this.Errorno == -1)
			{
				return true;
			}
			if (isToast)
			{
				switch (this.Errorno)
				{
				}
			}
			return false;
		}

		private bool checkIfProductIsAvailableForSubscriptionManager(string receipt)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
			if (!dictionary.ContainsKey("Store") || !dictionary.ContainsKey("Payload"))
			{
				UnityEngine.Debug.Log("The product receipt does not contain enough information");
				return false;
			}
			string a = (string)dictionary["Store"];
			string text = (string)dictionary["Payload"];
			if (text == null)
			{
				return false;
			}
			if (!(a == "GooglePlay"))
			{
				return a == "AppleAppStore" || a == "AmazonApps" || a == "MacAppStore";
			}
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)MiniJson.JsonDecode(text);
			if (!dictionary2.ContainsKey("json"))
			{
				UnityEngine.Debug.Log("The product receipt does not contain enough information, the 'json' field is missing");
				return false;
			}
			Dictionary<string, object> dictionary3 = (Dictionary<string, object>)MiniJson.JsonDecode((string)dictionary2["json"]);
			if (dictionary3 == null || !dictionary3.ContainsKey("developerPayload"))
			{
				UnityEngine.Debug.Log("The product receipt does not contain enough information, the 'developerPayload' field is missing");
				return false;
			}
			Dictionary<string, object> dictionary4 = (Dictionary<string, object>)MiniJson.JsonDecode((string)dictionary3["developerPayload"]);
			if (dictionary4 == null || !dictionary4.ContainsKey("is_free_trial") || !dictionary4.ContainsKey("has_introductory_price_trial"))
			{
				UnityEngine.Debug.Log("The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
				return false;
			}
			return true;
		}
	}
}
