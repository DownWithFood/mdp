using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TowerMenu))]
public class Tower : MonoBehaviour {
	
	public GameObject Projectile;
	
	public string ActiveModelName
	{
		get { return m_ActiveModelName; }
	}
	private string m_ActiveModelName;
	
	// Firing cooldown in seconds
	public float BaseCooldown;
	public float Level1Cooldown;
	public float Level2Cooldown;
	
	private TowerMenu m_Menu;
    private Transform m_ActiveModel;

    private Color m_TargetColor;
    private EnzymeManager m_EnzymeManager;

	private float m_Cooldown;
	private float m_CurrentCooldown;
	private bool m_CanFire;
		
	// Use this for initialization
	void Start () {
		m_Cooldown = BaseCooldown;
		m_CurrentCooldown = m_Cooldown;
		m_CanFire = true;

        m_EnzymeManager = GameObject.Find("Managers").GetComponent<EnzymeManager>();
		
		m_Menu = gameObject.GetComponent<TowerMenu>();
		
		foreach (Transform child in transform)
		{
			child.localPosition = Vector3.zero;
			child.gameObject.SetActive(false);
		}
		
		SetActiveModel("Base");		
	}
	
	// Update is called once per frame
	void Update () {
		if (m_CanFire)
		{
            Fire();
		}
		else
		{
			m_CurrentCooldown -= Time.deltaTime;
			if (m_CurrentCooldown <= 0f)
			{
				m_CanFire = true;
				m_CurrentCooldown = m_Cooldown;
			}
		}
	}
	
	void OnMouseUpAsButton()
	{
		m_Menu.IsEnabled = !m_Menu.IsEnabled;
	}

    public void SetColor(Color color)
    {
        m_TargetColor = color;

        foreach (Transform child in transform)
        {
            Transform circle = child.FindChild("Circle");
            circle.renderer.materials[0].color = m_TargetColor;
        }
    }
	
	private void SetActiveModel(string name)
	{
		if(m_ActiveModelName != null)
			m_ActiveModel.gameObject.SetActive(false);
		
		m_ActiveModelName = name;
        m_ActiveModel = transform.FindChild(m_ActiveModelName);
		m_ActiveModel.gameObject.SetActive(true);
	}
	
	private void Fire()
	{
        Enzyme target = AcquireTarget();
        if (target)
        {
            transform.LookAt(target.transform);
            transform.FindChild(m_ActiveModelName).animation.Play("Default Take", PlayMode.StopAll);

            GameObject bulletObject = Instantiate(Projectile, transform.position, transform.rotation) as GameObject;
            bulletObject.renderer.material.color = m_TargetColor;

            Bullet bullet = bulletObject.GetComponent<Bullet>();
            bullet.Target = target.gameObject;

            target.IsTargetted = true;

            m_CanFire = false;
        }
	}

    // Returns closest enzyme of TargetColor's type 
    // that isn't already targetted
    // or null if there are no valid targets
    private Enzyme AcquireTarget()
    {
        IList<Enzyme> enzymes = m_EnzymeManager.GetEnzymes(m_TargetColor);
        if (enzymes == null)
        {
            return null;
        }

        Enzyme closestEnzyme = null;
        float closestDistance = float.MaxValue;

        foreach (Enzyme e in enzymes)
        {
            if (!e.IsTargetted)
            {
                float distance = MDPUtility.DistanceSquared(gameObject, e.gameObject);

                if (distance < closestDistance)
                {
                    closestEnzyme = e;
                    closestDistance = distance;
                }
            }
        }

        return closestEnzyme;
    }
	
	public void UpgradeSpeed()
	{
		switch (m_ActiveModelName)
		{
		case "Base":
			SetActiveModel("Speed1");
			m_Cooldown = Level1Cooldown;
			AdjustAnimationSpeed(m_Cooldown);
			break;
		case "Speed1":
			SetActiveModel("Speed2");
			m_Cooldown = Level2Cooldown;
			AdjustAnimationSpeed(m_Cooldown);
			break;
		default:
			break;
		}
	}
	
	private void AdjustAnimationSpeed(float newCooldown)
	{
        foreach (AnimationState state in transform.FindChild(m_ActiveModelName).animation)
        {
            state.speed = BaseCooldown / newCooldown;
        }
	}
	
	public void UpgradePower()
	{
		switch (m_ActiveModelName)
		{
		case "Base":
			SetActiveModel("Power1");
			break;
		case "Power1":
			SetActiveModel("Power2");
			break;
		default:
			break;
		}
	}
}
