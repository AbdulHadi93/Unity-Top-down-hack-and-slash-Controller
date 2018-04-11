using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour {
    float minStopDis=2,maxStopDis=4,minmovedelay=1,maxmovedelay=3;
    float StopingDistance;
    TPlayerMovement player;
    NavMeshAgent agent;
    Animator anim;
    public bool ismove = true, isattack = false, isdodge = false;
    public bool isdead = false;
    public bool beingattacked = false,startfight = false;
    float distance;
    [Range(0.0f, 1.0f)]
    public float Difficulty;
    bool colliderhit = false;
    public GameObject blood;
    enum leftright { left,right,idle};
    leftright dir;
    Finish finish;
    public ParticleSystem slash;
    public EnemySlashTransform[] slashtransforms;
    int attackcount = 0;
    audiomanager am;
    public GameObject hitimage;
    int hitpoints;
    //public HitBox box;
    public int getattackcount
    {
        get { return attackcount; }
        set { attackcount = value; }
    }
    
	// Use this for initialization
	void Start () {
        am = GetComponent<audiomanager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<TPlayerMovement>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        StopingDistance = Random.Range(minStopDis,maxStopDis);
        StartCoroutine(AiDifficulty());
        StartCoroutine(delayleftright());
        finish = FindObjectOfType<Finish>();
	}
	// Update is called once per frame
	void Update () {
        if (!isdead && startfight)
        {
            Dodge();
            Attack();
            move();
            moveRightLeft();
        }
	}
    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f)
        {
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }
    void move()
    {
            agent.destination = player.transform.position;
            distance = Vector3.Distance(transform.position, player.transform.position);
            Turn();
            if (distance > StopingDistance && ismove&&!isattack&&!anim.GetBool("Turn")&&!isdodge)
            {
                StopCoroutine(MoveDelay());
                agent.updatePosition = true;
                agent.updateRotation = true;
                agent.speed = Mathf.Lerp(agent.speed, 1, 3 * Time.deltaTime);
                anim.SetFloat("movey", agent.speed);
            }
            else if (distance < StopingDistance - 1 && ismove && !isattack && !anim.GetBool("Turn")&&!isdodge)
            {
                    StopCoroutine(MoveDelay());
                    agent.updatePosition = false;
                    agent.updateRotation = false;
                    anim.SetFloat("movey", Mathf.Lerp(anim.GetFloat("movey"), -1, 3 * Time.deltaTime));
                    checkBackCollision();
            }
            else if(!isattack&&!isdodge)
            {
                if (colliderhit)
                {
                    ismove = true;
                    colliderhit = false;
                }
                StartCoroutine(MoveDelay());
                agent.speed = Mathf.Lerp(agent.speed, 0, 7 * Time.deltaTime);
                agent.updateRotation = false;
                anim.SetFloat("movey", agent.speed);
            }
            
    }
    void checkCollisionDirection()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].tag != "Enemy" && colliders[i].tag != "Ground" && colliders[i].tag != "Player")
            {
                Vector3 perp = Vector3.Cross(transform.forward, colliders[i].transform.position - transform.position);
                float direction = Vector3.Dot(perp, transform.up);

                if (direction > 0f)
                {
                    //right
                    dir = leftright.left;
                }
                else if (direction < 0f)
                {
                    //left
                    dir = leftright.right;
                }
                else
                {
                    //nothing
                }
            }
        }
    }
    void moveRightLeft()
    {
        if (!ismove&&dir!=leftright.idle&&!isattack)
        {
            checkCollisionDirection();
            agent.updatePosition = true;
            agent.updateRotation = true;
            if (dir == leftright.left)
            {
                anim.SetFloat("movex", Mathf.Lerp(anim.GetFloat("movex"), -1, 3 * Time.deltaTime));
                Vector3 newpos = player.transform.position;
                newpos.y = transform.position.y;
                transform.LookAt(newpos);
            }
            else if (dir == leftright.right)
            {
                anim.SetFloat("movex", Mathf.Lerp(anim.GetFloat("movex"), 1, 3 * Time.deltaTime));
                Vector3 newpos = player.transform.position;
                newpos.y = transform.position.y;
                transform.LookAt(newpos);
            }
            else
                anim.SetFloat("movex", Mathf.Lerp(anim.GetFloat("movex"), 0, 3 * Time.deltaTime));
        }
        else
            anim.SetFloat("movex", Mathf.Lerp(anim.GetFloat("movex"), 0, 3 * Time.deltaTime));
    }
    IEnumerator delayleftright()
    {
        while (!isdead)
        {
            int rnd = Random.Range(0,3);
            yield return new WaitForSeconds(4);
                if (rnd ==0)
                    dir = leftright.left;
                else if (rnd ==2)
                    dir = leftright.right;
                else
                    dir = leftright.idle;
        }
    }
    void checkBackCollision()
    {
        RaycastHit hit;
        Vector3 point1 = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        Vector3 point2 = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        if (Physics.CapsuleCast(point1, point2, 0.1f, -transform.forward, out hit, 0.5f))
        {
            if (hit.collider.tag != "Ground"&&hit.collider.tag!="enemybone")
            {
                ismove = false;
                isdodge = false;
                colliderhit = true;
            }
        }
    }
    void Turn()
    {
        if (!agent.updateRotation)
        {
            float rightangle = Vector3.Angle(transform.right, player.transform.position - transform.position);
            float leftangle = Vector3.Angle(-transform.right, player.transform.position - transform.position);
            float forward = Vector3.Angle(transform.forward, player.transform.position - transform.position);
            if (forward > 15)
            {
                anim.SetBool("isTurn", true);
                if (rightangle < leftangle)
                    anim.SetFloat("Turn", 1);
                else if (leftangle < rightangle)
                    anim.SetFloat("Turn", -1);
            }
            else
            {
                anim.SetBool("isTurn", false);
            }
        }
        else
            anim.SetBool("isTurn", false);
    }
    IEnumerator MoveDelay()
    {
        if (!ismove)
            yield break;
            ismove = false;
            yield return new WaitForSeconds(Random.Range(minmovedelay,maxmovedelay));
            ismove = true;
    }
    public IEnumerator Death()
    {
        if (!isdead)
        {
            Instantiate(blood, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), Quaternion.identity);
            anim.SetBool("isTurn", false);
            agent.enabled = false;
            isdead = true;
            am.DeathSoundPlay();
            finish.checkwavedead();
            anim.SetInteger("Dead", Random.Range(1, 5));
            anim.SetTrigger("DeadTrigger");
            GetComponent<CapsuleCollider>().enabled = false;
            StartCoroutine(resethit());
            GetComponent<Health>().DisableSwordCollider();
            yield return new WaitForSeconds(0.5f);
            player.RemoveEnemy(this.gameObject);
            yield return new WaitForSeconds(.8f);
            EnableRagdoll();
        }
    }
    void EnableRagdoll()
    {
        agent.enabled = false;
        anim.enabled = false;
        Rigidbody[] rb = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rb.Length; i++)
        {
            rb[i].isKinematic = false;
        }
        
    }
    public void Hit()
    {
        StartCoroutine(resethit());
        if (!isdead)
        {
            Instantiate(blood,new Vector3(transform.position.x, transform.position.y+1, transform.position.z),Quaternion.identity);
            am.HitSoundPlay();

            anim.SetTrigger("Hit");
        }
    }
    IEnumerator resethit()
    {
        hitimage.SetActive(false);
        hitimage.SetActive(true);
        hitpoints += 10;
        finish.Totalhitpoints += hitpoints;
        hitimage.GetComponentInChildren<Text>().text = hitpoints.ToString();
        if (hitpoints > 10)
            yield break;
        yield return new WaitForSeconds(2);
        hitpoints = 0;
        hitimage.SetActive(false);
    }
    void Dodge()
    {
        if(beingattacked)
        {
            if (Random.Range(0, 1.5f) < Difficulty)
            {
                isdodge = true;
            }
            beingattacked = false;
        }
        if (isdodge)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
            anim.SetTrigger("Dodge");
            isdodge = false;
            ismove = true;
        }
    }
    void Attack()
    {
        if (isattack&&!isdodge)
        {
            ismove = false;
            StopCoroutine(MoveDelay());
            if (distance > 2.5f)
            {
                agent.updatePosition = true;
                agent.updateRotation = true;
                agent.speed = Mathf.Lerp(agent.speed, 1, 3 * Time.deltaTime);
            }
            else
            {
                agent.speed = Mathf.Lerp(agent.speed, 0, 7 * Time.deltaTime);
                agent.updatePosition = false;
                agent.updateRotation = false;
                if (isattack)
                {
                    anim.SetTrigger("Attack");
                    int i = Random.Range(0, 3);
                    anim.SetInteger("Attackrnd",i);
                    attackcount = i;
                    ismove = true;
                }
                isattack = false;
            }
            anim.SetFloat("movey", agent.speed);
        }
        if (agent.updatePosition == false)
        {
            agent.nextPosition = transform.position;
            transform.position = new Vector3(transform.position.x, agent.nextPosition.y, transform.position.z);
        }
        if (colliderhit)
        {
            agent.updatePosition = true;
            agent.updateRotation = true;
        }
    }
    IEnumerator AiDifficulty()
    {
        while (!isdead)
        {
            yield return new WaitForSeconds(1.5f-Difficulty);
            if (Random.Range(0, 2f) <= Difficulty&&startfight)
            {
                isattack = true;
            }
        }
    }
	public void PlaySlash()
    {
        Transform st = slash.transform;
        st.localPosition = slashtransforms[attackcount].position;
		st.localEulerAngles = slashtransforms[attackcount].rotation;
		st.localScale = slashtransforms[attackcount].scale;
        slash.Play();
    }
}
[System.Serializable]
public class EnemySlashTransform : System.Object
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}
