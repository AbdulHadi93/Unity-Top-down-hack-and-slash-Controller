using UnityEngine;
using System.Collections;

public class PrivacyPolicy : MonoBehaviour 
{
	private const string AGREED_POLICY = "AGREED_POLICY";
	[SerializeField]
	private GameObject _mainText;
	[SerializeField]
	private GameObject _policyLinks;
	[SerializeField]
	private GameObject _UI;
	[SerializeField]
	private GameObject _UIBanner;

	public static bool TempAgreedPolicy = false;

    public static PrivacyPolicy Instance;

	// Use this for initialization
	void Start () 
	{
        Instance= this;
        Reset();
	}

    void OnEnable()
    {
        Instance= this;
    }

    public void Reset()
    {
        _UI.SetActive (false);
        _mainText.SetActive (true);
        _policyLinks.SetActive (false);
        _UIBanner.SetActive (true);
    }

    public void CloseAll()
    {
        ClosePolicyGUI();
        ClosePolicyBannerGUI();
    }
	
	public void AgreePolicy()
	{
		AgreedPolicy = 1;
		Application.LoadLevel ("Game");
	}

	public static void AgreePolicyReset()
	{
		AgreedPolicy = 0;
	}

	public void OnButtonLinks()
	{
		_mainText.SetActive (false);
		_policyLinks.SetActive (true);
	}

	static int AgreedPolicy
	{
		get
		{
			return PlayerPrefs.GetInt( AGREED_POLICY , 0);
		}

		set
		{
			 PlayerPrefs.SetInt( AGREED_POLICY , value);
		}
	}

	public void ButtonGooglePrivacyPolicy()
	{
		Application.OpenURL ("http://www.google.com/policies/privacy/");
	}

	public void ButtonChartboostPrivacyPolicy()
	{
		Application.OpenURL ("https://answers.chartboost.com/hc/en-us/articles/200780269-Privacy-Policy");
	}


	public void ButtonVunglePrivacyPolicy()
	{
		Application.OpenURL ("https://vungle.com/privacy/");
	}

    public void ButtonApplovinPrivacyPolicy()
    {
        Application.OpenURL ("https://www.applovin.com/privacy");
    }
    public void ButtonFlurryPrivacyPolicy()
    {
        Application.OpenURL ("https://developer.yahoo.com/flurry/end-user-opt-out/");
    }


	public void ActivatePolicyGUI()
	{
		gameObject.SetActive (true);
		_UI.SetActive (true);
		_mainText.SetActive (true);
		_policyLinks.SetActive (false);
		_UIBanner.SetActive (false);
		TempAgreedPolicy = true;
	}

    public void ClosePolicyGUI()
	{
        if(_UI != null)
	    	_UI.SetActive (false);
	
	}

    public void ClosePolicyBannerGUI()
	{
        if(_UIBanner != null)
		    _UIBanner.SetActive (false);
	}
}
