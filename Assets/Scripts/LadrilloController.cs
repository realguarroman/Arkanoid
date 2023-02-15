using Fusion;
using UnityEngine;
public class LadrilloController : NetworkBehaviour
{
    NetworkCharacterControllerPrototype bolaNetwork;
    BolaController bolaController;

    private void Awake() {
        var bola = GameObject.Find("Bola(Clone)");
        bolaNetwork = bola.GetComponent<NetworkCharacterControllerPrototype>();
        bolaController = bola.GetComponent<BolaController>();
    }

    public override void FixedUpdateNetwork()
    {
        Vector3 pos = bolaNetwork.ReadPosition();

        //Vemos si la bola ha colisionado con este ladrillo
        //en caso afirmativo lo destruimos

        float RectWidth = transform.localScale.x;
        float RectHeight = transform.localScale.y;
        float RectX = transform.position.x - (RectWidth / 2f);
        float RectY = transform.position.y - (RectHeight / 2f);

        float CircleRadius = 1f / 2f;
        float CircleX = pos.x;
        float CircleY = pos.y;

        float NearestX = Mathf.Max(RectX, Mathf.Min(CircleX, RectX + RectWidth));
        float NearestY = Mathf.Max(RectY, Mathf.Min(CircleY, RectY + RectHeight));

        float DeltaX = CircleX - NearestX;
        float DeltaY = CircleY - NearestY;
        bool intersection = (DeltaX * DeltaX + DeltaY * DeltaY) < (CircleRadius * CircleRadius);

        if (intersection)
        {
            if (NearestX == CircleX)
                bolaController.directionY = -bolaController.directionY;
            else if (NearestY == CircleY)
                bolaController.directionX = -bolaController.directionX;
            else {
                bolaController.directionX = -bolaController.directionX;
                bolaController.directionY = -bolaController.directionY;
            }

            Runner.Despawn(gameObject.GetComponent<NetworkObject>());
        }
    }
}


