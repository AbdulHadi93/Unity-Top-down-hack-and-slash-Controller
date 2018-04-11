using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CnControls;
using UnityEngine.AI;

public class TPlayerMovement : MonoBehaviour {
    CharacterController controller;
    Animator anim;
    Vector3 input = Vector3.zero;
    Transform maincamera;
    int combocounter = 0;
    public List<GameObject> enemies;
    public bool isattack = false,once=true;
    public ParticleSystem slash,trail;
    public SlashTransform[] slashtransforms;
    bool isdead = false,ismove=true;
    public bool isfall { get; set; }
	NavMeshAgent agent;
    audiomanager am;
    //public HitBox box;
	// Use this for initialization
    public int getcombocounter
    {
        get { return combocounter; }
        set { combocounter = value; }
    }
	void Start () {
        am = GetComponent<audiomanager>();
        GuiManager.instance.Player = this;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        maincamera = Camera.main.transform;
		agent = GetComponent<NavMeshAgent> ();
		agent.updatePosition = false;
		agent.updateRotation = false;
	}
	
	// Update is called once per frame
	void Update () {
        move();
//        if (isattack)
//            Keeplock();
        controller.Move(Vector3.down);
		transform.position = new Vector3(transform.position.x ,agent.nextPosition.y,transform.position.z);
		agent.nextPosition = transform.position;
	}
    void checkforwardCollision(Vector3 direction)
    {
        RaycastHit hit;
        Vector3 newpos = transform.position;
        newpos.y += 0.7f;
        if (Physics.Raycast(newpos, direction, out hit, 1f))
        {
            if (hit.collider.tag != "AvoidRaycast" )//&& hit.collider.tag != "enemybone" && hit.collider.tag != "Enemy" && hit.collider.tag != "Player"&&hit.collider.tag != "playerbone")
            ismove = false;
        }
        else
            ismove = true;
        Debug.DrawRay(newpos,direction*1.5f,Color.red);
    }
    void move()
    {
            input = new Vector3(CnInputManager.GetAxis("Horizontal"), CnInputManager.GetAxis("Vertical"));
            Vector3 movementVector = Vector3.zero;
            float mag = input.sqrMagnitude;
            if (mag > 0.0001f)
            {
                movementVector = maincamera.TransformDirection(input);
                movementVector.y = 0f;
                movementVector.Normalize();
                transform.forward = Vector3.Lerp(transform.forward, movementVector, 10 * Time.deltaTime);
                checkforwardCollision(movementVector);
            }
        if(ismove)
        {
            anim.SetFloat("move", Mathf.Lerp(anim.GetFloat("move"), mag, 10 * Time.deltaTime));
        }
        else
            anim.SetFloat("move", Mathf.Lerp(anim.GetFloat("move"), 0, 10 * Time.deltaTime));
    }
    void Keeplock()
    {
        if (enemies.Count == 0)
            return;
        Transform enemy=ClosestEnemy().transform;
        transform.LookAt(new Vector3(enemy.position.x,transform.position.y,enemy.position.z));
        float distance = Vector3.Distance(transform.position, enemy.transform.position);
        anim.SetFloat("distance", distance);
        if (distance > 3)
        {
            controller.Move(transform.forward * 15 * Time.deltaTime);
            trail.gameObject.SetActive(true);
        }
        else
            trail.gameObject.SetActive(false);
        if (once)
        {
            enemy.GetComponent<EnemyMovement>().beingattacked = true;
            once = false;
        }
    }
    public void ComboCounter()
    {
        if(ClosestEnemy()!=null)
        anim.SetFloat("distance", Vector3.Distance(transform.position, ClosestEnemy().transform.position));
        if (combocounter == 0)
            StartCoroutine(resetcombo());
       
        anim.SetTrigger("combo in");
    }
    IEnumerator resetcombo()
    {
        yield return new WaitForSeconds(4);
        combocounter = 0;
        anim.SetInteger("combo",combocounter);
    }
    public void ComboTrigger()
    {
            if (combocounter < 6)
            {
                combocounter++;
                anim.SetInteger("combo", combocounter);
            }
            else
            {
                combocounter = 0;
                anim.SetInteger("combo", combocounter);
            }
    }
    public void Dodge()
    {
        anim.SetTrigger("dodge");
    }
	public void Dash()
	{
		anim.SetTrigger("dash");
	}
	public void Slay()
	{
		anim.SetTrigger("slay");
	}
	public void Spin()
	{
		anim.SetTrigger("spin");
	}
	public void PlaySlash(int i)
    {
        Transform st = slash.transform;
        if (combocounter > 5)
            combocounter = 0;
        //if (combocounter != 2)
        //{
		st.localPosition = slashtransforms[i>5?i:combocounter].position;
		st.localEulerAngles = slashtransforms[i>5?i:combocounter].rotation;
		st.localScale = slashtransforms[i>5?i:combocounter].scale;
            slash.Play();
        //}
    }
    public void Death()
    {
        if (!isdead)
        {
            am.DeathSoundPlay();
            if (!isfall)
            {
                anim.SetTrigger("DeadTrigger");
                anim.SetInteger("Dead", Random.Range(1, 5));
                StartCoroutine(EnableRagdoll(1));
            }
            else
            {
                StartCoroutine(EnableRagdoll(0));
            }
                isdead = true;
            GuiManager.instance.gameover("dead");
        }
    }
    IEnumerator EnableRagdoll(float delay)
    {
        yield return new WaitForSeconds(delay);
        controller.enabled = false;
        anim.enabled = false;
        Rigidbody[] rb = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rb.Length; i++)
        {
            rb[i].isKinematic = false;
        }

    }
    void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "Enemy")
        {
            enemies.Add(hit.gameObject);
            hit.GetComponent<EnemyMovement>().startfight = true;
        }
    }
    void OnTriggerExit(Collider hit)
    {
        if (hit.tag == "Enemy")
        {
            enemies.Remove(hit.gameObject);
        }
    }
    GameObject ClosestEnemy()
    {
        if (enemies.Count == 0)
            return null;
        float closestdist=1000000000;
        GameObject closestenemy=null;
        for (int i = 0; i < enemies.Count; i++)
        {
            float newdis=Vector3.Distance(transform.position, enemies[i].transform.position);
            if ( newdis< closestdist)
            {
                closestenemy = enemies[i].gameObject;
                closestdist = newdis;
            }
        }
        return closestenemy;
    }
    public void RemoveEnemy(GameObject obj)
    {
        enemies.Remove(obj);
    }
    public void Hit()
    {
        am.HitSoundPlay();
        anim.SetTrigger("Hit");
    }
}
[System.Serializable]
public class SlashTransform : System.Object
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}
