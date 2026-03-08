using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f; // Velocidad a la que se mueve la cámara

    public float zoomSpeed = 5f;  // Sensibilidad de la rueda del ratón
    public float minZoom = 3f;    // Lo más cerca que puedes estar
    public float maxZoom = 15f;   // Lo más lejos que puedes alejarte

    public Tilemap mapBoundary;

    private Camera cam;
    private float mapMinX, mapMaxX, mapMinY, mapMaxY;

    void Start()
    {
        // Cogemos la referencia a la cámara en la que está este script
        cam = GetComponent<Camera>();

        // Si hemos asignado un mapa, calculamos dónde están sus bordes
        if (mapBoundary != null)
        {
            mapBoundary.CompressBounds(); // Limpia casillas fantasma invisibles
            Bounds bounds = mapBoundary.localBounds;
            
            mapMinX = bounds.min.x;
            mapMaxX = bounds.max.x;
            mapMinY = bounds.min.y;
            mapMaxY = bounds.max.y;
        }
    }

    void Update()
    {
        MoveCamera();
        ZoomCamera();
    }

    // LateUpdate se ejecuta después de que todo lo demás se haya movido en el Update
    void LateUpdate()
    {
        if (mapBoundary != null)
        {
            ClampCamera();
        }
    }

    void MoveCamera()
    {
        // GetAxisRaw pilla las flechas del teclado y también W, A, S, D
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Normalizamos para que no vaya más rápido al moverse en diagonal
        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;

        // Movemos la cámara independientemente de los FPS (Time.deltaTime)
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    void ZoomCamera()
    {
        // Detecta si la rueda del ratón gira hacia arriba (positivo) o abajo (negativo)
        float scrollData = Input.GetAxis("Mouse ScrollWheel");

        if (scrollData != 0.0f)
        {
            // Modificamos el tamaño ortográfico de la cámara (el zoom en 2D)
            cam.orthographicSize -= scrollData * zoomSpeed;

            // Clamp asegura que el zoom nunca baje de minZoom ni suba de maxZoom
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    void ClampCamera()
    {
        // 1. Calculamos cuánto mide la mitad de la pantalla de la cámara actualmente
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        // 2. Definimos los topes (Borde del mapa menos la mitad de la cámara)
        float limitMinX = mapMinX + camWidth;
        float limitMaxX = mapMaxX - camWidth;
        float limitMinY = mapMinY + camHeight;
        float limitMaxY = mapMaxY - camHeight;

        // (Seguridad extra por si haces tanto zoom out que la cámara es más grande que el mapa)
        if (limitMaxX < limitMinX) limitMinX = limitMaxX = (mapMinX + mapMaxX) / 2f;
        if (limitMaxY < limitMinY) limitMinY = limitMaxY = (mapMinY + mapMaxY) / 2f;

        // 3. Forzamos a la cámara a quedarse dentro de esos límites
        float clampedX = Mathf.Clamp(transform.position.x, limitMinX, limitMaxX);
        float clampedY = Mathf.Clamp(transform.position.y, limitMinY, limitMaxY);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}