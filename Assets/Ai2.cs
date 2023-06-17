using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Panda;

public class Ai2 : MonoBehaviour
{
    public Transform player;
    public Transform bulletSpawn;
    public Slider healthBar;
    public GameObject bulletPrefab;
    public LayerMask mascaraObstaculos;

    NavMeshAgent agent;
    public Vector3 destination;
    public Vector3 target;
    float health = 100.0f;
    float rotSpeed = 5.0f;
    float atrasoEntreTiros = 0.5f;
    bool PodeAtirar = false;

    float visibleRange = 80.0f;
<<<<<<< HEAD
    float shotRange = 40.0f;
=======
    float shotRange = 300.0f;
>>>>>>> parent of 97cae03 (feito)

    [Task]
    public void PickRandomDestination()
    {
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
        agent.SetDestination(dest);
        Task.current.Succeed();
    }
    [Task]
    public void MoveToDestination()
    {
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    [Task]
    public bool IsHealthLessThan(float health)
    {
        return this.health < health;
    }
    [Task]
    public bool Explode()
    {
        Destroy(healthBar.gameObject);
        Destroy(this.gameObject);
        return true;
    }

<<<<<<< HEAD
    [Task]
    public bool SeePlayer()
    {
=======
        [Task]
        public bool SeePlayer(){
             // Verificar se há linha de visão direta entre o inimigo e o jogador
>>>>>>> parent of 97cae03 (feito)
        RaycastHit hit;
        Vector3 direcaoJogador = player.position - transform.position;

        if (Physics.Linecast(transform.position, player.position, out hit, mascaraObstaculos))
        {
            // Um obstáculo está bloqueando a linha de visão
            return false;
        }
        else
        {
<<<<<<< HEAD
=======
            Debug.Log("Tem visão direta");
            // Não há obstáculo bloqueando a linha de visão
>>>>>>> parent of 97cae03 (feito)
            return true;
        }

    }

    [Task]
    public void TargetPlayer()
    {

    }

    [Task]
    public bool LookAtTarget()
    {
        Vector3 direcaoJogador = player.position - transform.position;
        direcaoJogador.y = 0f;

        
        if (direcaoJogador != Vector3.zero)
        {
            Quaternion novaRotacao = Quaternion.LookRotation(direcaoJogador);
            transform.rotation = Quaternion.Slerp(transform.rotation, novaRotacao, Time.deltaTime * 5f);
            Fire();
            return true;
        }
<<<<<<< HEAD
        else return false;
    }



    [Task]
    public void Fire()
    {

        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);

    }
=======

        [Task]
        public void TargetPlayer(){

        }

        [Task]
        public bool LookAtTarget(){
             // Calcular a direção do jogador em relação ao inimigo
            Vector3 direcaoJogador = player.position - transform.position;
            direcaoJogador.y = 0f; // Ignorar a componente y (altura)

            // Rotacionar o inimigo para olhar na direção do jogador
            if (direcaoJogador != Vector3.zero)
            {
                Quaternion novaRotacao = Quaternion.LookRotation(direcaoJogador);
                transform.rotation = Quaternion.Slerp(transform.rotation, novaRotacao, Time.deltaTime * 5f);
                return true;
            }
            else return false;
        }


        [Task]
        public void Fire(){
           
                GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
                bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward*2000);
            
        }
>>>>>>> parent of 97cae03 (feito)

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
<<<<<<< HEAD
        agent.stoppingDistance = shotRange - 5;
        InvokeRepeating("UpdateHealth", 5, 0.5f);
=======
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        InvokeRepeating("UpdateHealth",5,0.5f);
>>>>>>> parent of 97cae03 (feito)
    }

    void Update()
    {
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)health;
<<<<<<< HEAD
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);
=======
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
>>>>>>> parent of 97cae03 (feito)
    }

    void UpdateHealth()
    {
        if (health < 100)
            health++;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }

}