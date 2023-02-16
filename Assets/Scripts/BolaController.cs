using Fusion;
using UnityEngine;
public class BolaController : NetworkBehaviour
{
    enum BolaStates { Idle, Active }

    private NetworkCharacterControllerPrototype _cc;

    ComManager managerAll;
    LivesController lives;

    private float LWall;
    private float RWall;
    private float TWall;
    private float BWall;

    [Networked]
    public float directionX { get; set; } = 0;

    [Networked]
    public float directionY { get; set; } = 0;

    [Networked]
    public int state { get; set; } = 0;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();

        managerAll = GameObject.Find("ComManager").GetComponent<ComManager>();
        lives = GameObject.Find("Lives(Clone)").GetComponent<LivesController>();

        var fieldTransf = GameObject.Find("Field").transform;
        var fieldScaleX = fieldTransf.localScale.x / 2;
        var fieldScaleY = fieldTransf.localScale.y / 2;
        var fieldPositionX = fieldTransf.position.x;
        var fieldPositionY = fieldTransf.position.y;

        LWall = fieldPositionX - fieldScaleX;
        RWall = fieldPositionX + fieldScaleX;
        BWall = fieldPositionY - fieldScaleY;
        TWall = fieldPositionY + fieldScaleY;
    }

    public void ChangeDirectionX() {
        directionX = -directionX;
    }

    public void ChangeDirectionY(){
        directionY = -directionY;
    }

    public void SetIdle(Vector3 pos) {
        directionX = 1; directionY = 1;
        state = (int)BolaStates.Idle;

        pos.y += 1;
        _cc.TeleportToPosition(pos);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data)) {
            if (state == (int)BolaStates.Idle &&
                data.direction.x == float.NegativeInfinity) {
                state = (int)BolaStates.Active;
            }
        }

        if (state == (int)BolaStates.Active) {
            var currentPos = _cc.ReadPosition();

            if (currentPos.y + transform.localScale.y / 2f >= TWall ||
                currentPos.y - transform.localScale.y / 2f <= BWall) {
                lives.ReduceLives();
                Vector3 pos = _cc.ReadPosition();

                if (managerAll.host != null) {
                    pos = managerAll.host.Value.Value
                        .gameObject.GetComponent<RaquetaController>()
                        .SetIdle();
                }
                if (managerAll.client != null) {
                    managerAll.client.Value.Value
                        .gameObject.GetComponent<RaquetaController>()
                        .SetIdle();
                }

                SetIdle(pos);
                return;
            }
            if (currentPos.x + transform.localScale.x / 2f >= RWall ||
                currentPos.x - transform.localScale.x / 2f <= LWall)
                directionX = -directionX;

            _cc.TeleportToPosition(_cc.ReadPosition()
                + (10 * new Vector3(directionX, directionY, 0) * Runner.DeltaTime));
        }
    }
}


