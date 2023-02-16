using Fusion;
using UnityEngine;
public class RaquetaController : NetworkBehaviour
{
    enum RaquetaStates { Idle, InActive, Active }

    NetworkCharacterControllerPrototype bolaNetwork;
    BolaController bolaController;

    ComManager managerAll;

    private NetworkCharacterControllerPrototype _cc;
    NetworkObject mynetwork;

    private float LWall;
    private float RWall;

    [Networked]
    public float directionX { get; set; } = 0;

    [Networked]
    public int state { get; set; } = 0;

    private void Awake() {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
        mynetwork = GetComponent<NetworkObject>();

        managerAll = GameObject.Find("ComManager").GetComponent<ComManager>();

        var bola = GameObject.Find("Bola(Clone)");
        bolaNetwork = bola.GetComponent<NetworkCharacterControllerPrototype>();
        bolaController = bola.GetComponent<BolaController>();

        var fieldTransf = GameObject.Find("Field").transform;
        var raquetaScale = gameObject.transform.localScale.y / 2;
        var fieldScale = fieldTransf.localScale.x / 2;
        var fieldPosition = fieldTransf.position.x;

        LWall = fieldPosition - fieldScale + raquetaScale + 1.2f;
        RWall = fieldPosition + fieldScale - raquetaScale - 1.2f;
    }

    public Vector3 SetIdle() {
        state = (int)RaquetaStates.Idle;
        directionX = 0;
        mynetwork.AssignInputAuthority(managerAll.host.Value.Key);
        return _cc.ReadPosition();
    }

    public override void FixedUpdateNetwork()
    {
        if (state == (int)RaquetaStates.Idle &&
            GetInput(out NetworkInputData data) &&
            data.direction.x == float.NegativeInfinity) {
            state = (int)RaquetaStates.InActive;

            if (mynetwork == managerAll.host.Value.Value)
                mynetwork.AssignInputAuthority(managerAll.host.Value.Key);
            else
                mynetwork.AssignInputAuthority(managerAll.client.Value.Key);
        }
                     

        if (state != (int)RaquetaStates.Idle) {

            if (GetInput(out NetworkInputData data1) &&
                data1.direction.x != float.NegativeInfinity &&
                data1.direction.x != float.PositiveInfinity)
                directionX = data1.direction.x;

            Vector3 pos = bolaNetwork.ReadPosition();
            Vector3 raquetaPos = _cc.ReadPosition();

            //Vemos si la bola ha colisionado con este ladrillo
            //en caso afirmativo lo destruimos

            float RectWidth = transform.localScale.y;
            float RectHeight = transform.localScale.x;
            float RectX = raquetaPos.x - (RectWidth / 2f);
            float RectY = raquetaPos.y - (RectHeight / 2f);

            float CircleRadius = 1f / 2f;
            float CircleX = pos.x;
            float CircleY = pos.y;

            float NearestX = Mathf.Max(RectX, Mathf.Min(CircleX, RectX + RectWidth));
            float NearestY = Mathf.Max(RectY, Mathf.Min(CircleY, RectY + RectHeight));

            float DeltaX = CircleX - NearestX;
            float DeltaY = CircleY - NearestY;
            bool intersection = (DeltaX * DeltaX + DeltaY * DeltaY) < (CircleRadius * CircleRadius);

            if (state == (int)RaquetaStates.Active && intersection)
            {
                if (NearestX == CircleX)
                    bolaController.ChangeDirectionY();
                else if (NearestY == CircleY)
                    bolaController.ChangeDirectionX();
                else {
                    bolaController.ChangeDirectionX();
                    bolaController.ChangeDirectionY();
                }

                state = (int)RaquetaStates.InActive;
            }
            else if (state == (int)RaquetaStates.InActive && !intersection)
            {
                state = (int)RaquetaStates.Active;
            }

            var currentPos = raquetaPos.x;
            if (currentPos + 1 > RWall || currentPos - 1 < LWall) directionX *= -1;

            _cc.TeleportToPosition(raquetaPos + (10 *
                new Vector3(directionX, 0, 0) * Runner.DeltaTime));
        }
    }
}


