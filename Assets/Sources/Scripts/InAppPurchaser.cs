using System;
using UnityEngine;
using UnityEngine.Purchasing;
using BackEnd;

public class InAppPurchaser : MonoBehaviour, IStoreListener
{
    public static InAppPurchaser instance;

    private static IStoreController storeController;
    private static IExtensionProvider extensionProvider;

    #region 상품ID
    // 상품ID는 구글 개발자 콘솔에 등록한 상품ID와 동일하게 해주세요.

    // google productId ==============================
    // remove ads
    public const string removeAds = "";        // remove Ads
    #endregion

    void Awake () {
		instance = this;
	}

	void Start ()
	{
		InitializePurchasing ();
	}

	bool IsInitialized ()
	{
		return (storeController != null && extensionProvider != null);
	}

	void InitializePurchasing ()
	{
		//if (IsInitialized ())
		//	return;

		//Debug.Log ("##### InitializePurchasing : Start");
		//

		//var module = StandardPurchasingModule.Instance ();

		//ConfigurationBuilder builder = ConfigurationBuilder.Instance (module);
        //// remove ads
        //builder.AddProduct(removeAds, ProductType.NonConsumable); //, new IDs {{ removeAds, GooglePlay.Name }});

		//UnityPurchasing.Initialize (this, builder);
		Debug.Log ("##### InitializePurchasing : Initialize");
	}

	void BuyProductID (string _productId)
	{
		try 
        {
			if (IsInitialized ()) 
            {
				Product p = storeController.products.WithID (_productId);
				if (p != null) 
                {
					if (p.availableToPurchase) 
                    {
						if (p.definition.type == ProductType.NonConsumable && p.hasReceipt) 
                        {
							MessagePopManager.instance.ShowPop("이미 구매한 상품입니다");
                            // TODO : TEST 기능 (이미 구매한 상품이면 광고 지우기)
                            AdsManager.instance.SetRemoveAds();
                            BackEndServerManager.instance.SetRemoveAds();
                        } 
                        else 
                        {
							Debug.Log (string.Format ("Purchasing product asychronously: '{0}'", p.definition.id));
							storeController.InitiatePurchase (p);							
						}
					}
				} 
                else 
                {
					Debug.Log ("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");					
					MessagePopManager.instance.ShowPop("구매 실패");
				}
			} 
            else 
            {
				Debug.Log ("BuyProductID FAIL. Not initialized.");
				MessagePopManager.instance.ShowPop("구매 실패");
			} 
		} 
        catch (Exception e) 
        {
			Debug.Log ("BuyProductID: FAIL. Exception during purchase. " + e);
			MessagePopManager.instance.ShowPop("구매 실패");
		}
	}

	public void OnInitialized (IStoreController _sc, IExtensionProvider _ep)
	{
        Debug.Log("OnInitialized: PASS");
        storeController = _sc;
		extensionProvider = _ep;
	}

	public void OnInitializeFailed (InitializationFailureReason reason)
	{
		MessagePopManager.instance.ShowPop("OnInitializeFailed : \n" + reason);
	}

	// ====================================================================================================
#region 영수증 검증
    /* 
     *
	 */
	public PurchaseProcessingResult ProcessPurchase (PurchaseEventArgs args)
	{
		// 뒤끝 영수증 검증 처리    
        BackendReturnObject validation = Backend.Receipt.IsValidateGooglePurchase(args.purchasedProduct.receipt, "receiptDescription");

        string msg = "";

        // 영수증 검증에 성공한 경우
        if (validation.IsSuccess())
        {
            // 구매 성공한 제품에 대한 id 체크하여 그에맞는 보상 
            // A consumable product has been purchased by this user.
            if (String.Equals(args.purchasedProduct.definition.id, removeAds, StringComparison.Ordinal))
            {
                msg = string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id);
                MessagePopManager.instance.ShowPop(msg);
                Debug.Log(msg);

				AdsManager.instance.SetRemoveAds();
                BackEndServerManager.instance.BuyRemoveAdsSuccess();
            }
        }
        // 영수증 검증에 실패한 경우 
        else
        {
            // Or ... an unknown product has been purchased by this user. Fill in additional products here....
            msg = string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id);
            MessagePopManager.instance.ShowPop(msg);
            Debug.Log(msg);

            BackEndServerManager.instance.BuyRemoveAdsFailed();
        }

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed.
		return PurchaseProcessingResult.Complete;
	}
#endregion

    // ====================================================================================================	

	public void OnPurchaseFailed (Product product, PurchaseFailureReason failureReason)
	{		
		MessagePopManager.instance.ShowPop("구매 실패\n" + product.definition.storeSpecificId + "\n" + failureReason);
	}

	// ==================================================
	public void BuyRemoveAds () {
		BuyProductID (removeAds);
	}
}