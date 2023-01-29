#if !OS_OPERATINGSYSTEM
#define OS_OPERATINGSYSTEM
#define OS_MSWINDOWS
#define OS_64BITS
#endif

#define PANDA

//----constantes y tipos-----
#if OS_MSWINDOWS
using RTT_Time = System.Int64;
using HRT_Time = System.Int64;
#elif OS_LINUX
#elif OS_OSX
#elif OS_ANDROID
#endif

using UnityEngine;
using UnityEngine.AI;
using Panda;
using GAIA;
using System.Diagnostics;

public class NPC1_BT : MonoBehaviour {

    // GameObject attributes
    private float range;
    private float speed;
    private int ladrillos_counter;
    private int colliding_with_bricks;

    private Vector3 target;

    private float WRight;
    private float WLeft;
    private float WTop;
    private float WBottom;

    private Stopwatch stopwatch;
    private Stopwatch stopwatch2;

    public GameObject brick_prefab;
    private GameObject ladrillos_set;
    private Animation animation_comp;

    bool can_play;
    bool is_dead;
    bool not_laying_brick;

    private GAIA_Manager manager;               // Instatiates the manager
    public string NameFile;

    // Start is called before the first frame update
    void Start() {
        manager = GAIA_Controller.INSTANCE.m_manager;

#if (PANDA)
        manager.createBT(gameObject, NameFile);
#endif

        range = 0.1f;
        speed = 2;
        ladrillos_counter = 36;
        colliding_with_bricks = 0;

        can_play = false;
        is_dead = false;
        not_laying_brick = true;

        var temp = GameObject.Find("Field" + tag);
        WRight = temp.transform.position.x + (temp.transform.localScale.x / 2) - 2f;
        WLeft = temp.transform.position.x - (temp.transform.localScale.x / 2) + 2f;
        WTop = temp.transform.position.y + (temp.transform.localScale.y / 2) - 4f;
        WBottom = temp.transform.position.y - (temp.transform.localScale.y / 5);

        set_target();
        visible(false);

        ladrillos_set = GameObject.Find("Ladrillos" + tag);

        stopwatch = new Stopwatch();
        stopwatch2 = new Stopwatch();

        animation_comp = GetComponent<Animation>();
    }

    private void visible(bool visibility) {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(visibility);
        }
    }

    public void OnEnable() {
        if (stopwatch != null) {
            stopwatch.Restart();
            can_play = true;
            visible(true);
        }
    }

    public void OnDisableFunc() {
        can_play = false;
        visible(false);
        enabled = false;
    }

    void set_target() {
        target = new Vector3(
        Random.Range(WLeft, WRight),
        Random.Range(WBottom, WTop),
        transform.position.z);
    }

    [Task]
    bool Wandering() {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < range)
            set_target();

        return true;
    }

    [Task]
    bool CanLayBrick()
    {
        if (stopwatch.ElapsedMilliseconds > 5000) {
            if (colliding_with_bricks > 0) stopwatch.Restart();
            else {
                stopwatch.Reset();
                return true;
            }
        }

        return false;
    }

    [Task]
    bool LayBrick() {
        not_laying_brick = false;

        animation_comp.Stop();
        animation_comp.Play("LayingBrick");
        return true;
    }

    public void lbAnimFinishedFunc()
    {
        var pos = new Vector3(
            transform.position.x,
            transform.position.y,
            -1.5f);
        var rot = new Quaternion();

        GameObject new_brick = Instantiate(brick_prefab, pos, rot);

        new_brick.name = $"Ladrillo{tag}_{ladrillos_counter++}";
        new_brick.tag = tag;
        new_brick.transform.parent = ladrillos_set.transform;

        not_laying_brick = true;
    }

    [Task]
    bool NotLayingBrick() {
        return not_laying_brick;
    }

    [Task]
    bool CanPlay() {
        return can_play;
    }

    [Task]
    bool IsDead() {
        return is_dead;
    }

    [Task]
    bool IsAlive() {
        return !is_dead;
    }

    [Task]
    bool TryRespaw() {
        if (stopwatch2.ElapsedMilliseconds > 5000) {
            stopwatch2.Reset();

            visible(true);
            animation_comp.Stop();
            animation_comp.Play("Spawning");

            return true;
        }

        return false;
    }

    public void spawnAnimFinishedFunc() {
        is_dead = false;
    }

    public void OnTriggerEnter2DFunc(Collider2D other)
    {
        if (other.name.StartsWith("Ladrillo")) {
            colliding_with_bricks++;
        }
        else if (
            other.name.StartsWith("Bola") ||
            other.name.StartsWith("Raqueta")) {
            is_dead = true;

            animation_comp.Stop();
            animation_comp.Play("Exploding");
        }
    }

    public void expAnimFinishedFunc()
    {
        visible(false);
        stopwatch2.Start();
    }

    public void OnTriggerExit2DFunc(Collider2D other) {
        if (other.name.StartsWith("Ladrillo")) {
            colliding_with_bricks--;
        }
    }
}
