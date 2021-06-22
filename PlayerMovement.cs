using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PlayerMovement : MonoBehaviour
{
    private RaycastHit hit;
    private Vector3 camera_offset;
    Vignette vig;
    ChromaticAberration chromaticAberration;
    public Transform render;
    public CharacterController controller;
    public Transform cameraObject;
    public Transform groundChecker;
    public Transform topObstacleChecker;
    public Transform body;
    public LayerMask groundMask;
    [Range(0f, 20f)] public float playerSpeed = 10f;//Скорость перемещения игрока
    [Range(1f, 20f)] public float acceleration = 20f;//Ускорение
    public float gravity = -9.80f;//Гравитация
    [Range(3f, 10f)] public float jumpHeight = 10f;
    Vector3 velocity;
    public Camera cam;
    bool isGrounded;
    bool isObstacle;

    //headbobber
    [SerializeField] private bool _enableHeadbobbing = true;
    private float timer = 0.0f;
    public float bobbingSpeed = 0.18f;
    public float bobbingAmount = 0.2f;
    public float midpoint = 2.0f;
    //headbobber

    float previousSpeed { get; set; }//дополнительная переменная для хранения первоначальной скорости player

    void Start()
    {
        Volume volume = render.GetComponent<Volume>();
        Vignette tmpVig;
        ChromaticAberration tmpChrm;

        previousSpeed = playerSpeed;
        cameraObject.rotation = Quaternion.Euler(0f,
            transform.rotation.y,
            transform.rotation.z);
        cam.fieldOfView = 60f;
        camera_offset = cameraObject.localPosition;
        
        if(volume.profile.TryGet<Vignette>(out tmpVig))
        {
            vig = tmpVig;
        }
        if(volume.profile.TryGet<ChromaticAberration>(out tmpChrm))
        {
            chromaticAberration = tmpChrm;
        }
        vig.intensity.value = 0.150f;
        chromaticAberration.intensity.value = 0.09f;
        
    }

    void Update()
    {
        float waveslice = 0.0f;
        float xPlayer = Input.GetAxis("Horizontal");
        float zPlayer = Input.GetAxis("Vertical");

        isGrounded = Physics.CheckBox(groundChecker.position, new Vector3(0.5f, 1f, 0.5f), Quaternion.Euler(0f,0f,0f), groundMask.value);
        isObstacle = Physics.CheckBox(topObstacleChecker.position, new Vector3(0.05f, 1f, 0.05f), Quaternion.Euler(0f, 0f, 0f), groundMask.value);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5f;
        }
        if (isObstacle && velocity.y > 0)
        {
            velocity.y = 0f;
        }
        StartCoroutine(SpeedDecreasingInJumping());
        StartCoroutine(Jump());
        StartCoroutine(Run());
        StartCoroutine(Crouch());
        StartCoroutine(SetFieldOfView());
        StartCoroutine(CalculatePlayerMoving(xPlayer, zPlayer));
        StartCoroutine(Headbobbing(xPlayer, zPlayer, waveslice));

       
    }

    IEnumerator Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isObstacle)//прыжок
        {
            if (!isObstacle)
            { 
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            if(isObstacle && isGrounded)
            {
                velocity.y = 0f;
            }
        }
        yield return null;
    }

    IEnumerator Headbobbing(float xPlayer, float zPlayer, float waveslice)
    {
        if (_enableHeadbobbing)
        {
            if (Mathf.Abs(xPlayer) == 0 && Mathf.Abs(zPlayer) == 0)
            {
                timer = 0.0f;
                yield return null;
            }
            else
            {
                waveslice = Mathf.Sin(timer);
                timer = timer + bobbingSpeed;
                yield return null;
                if (timer > Mathf.PI * 2)
                {
                    timer = timer - (Mathf.PI * 2);
                    yield return null;
                }
            }
            yield return null;
            Vector3 v3T = cameraObject.localPosition;
            if (waveslice != 0)
            {
                float translateChange = waveslice * bobbingAmount;
                float totalAxes = Mathf.Abs(xPlayer) + Mathf.Abs(zPlayer);
                totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
                translateChange = totalAxes * translateChange;
                v3T.y = midpoint + translateChange;
                yield return null;
            }
            else
            {
                v3T.y = midpoint;
                yield return null;
            }
            cameraObject.localPosition = v3T;
        }
        yield return null;
    }

    IEnumerator Run()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && isGrounded)//ускорение
        {
            playerSpeed = Mathf.Lerp(playerSpeed, acceleration, Time.deltaTime * 5f);//Плавное ускорение
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 120f, Time.deltaTime * 2f);//Изменение угла видимости камеры
            vig.intensity.value = Mathf.Lerp(vig.intensity.value, 0.458f, Time.deltaTime * 2f);
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, 0.794f, Time.deltaTime * 2f);
            yield return null;
        }
        else
        {
            playerSpeed = Mathf.Lerp(playerSpeed, previousSpeed, Time.deltaTime * 5f);//Плавное замедление
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, Time.deltaTime * 2f);
            vig.intensity.value = Mathf.Lerp(vig.intensity.value, 0.150f, Time.deltaTime);
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, 0.09f, Time.deltaTime * 2f);
            yield return null;
        }
        yield return null;
    }

    IEnumerator Crouch()//Присед
    {
        RaycastHit obHit;
        bool tmp = Physics.Raycast(transform.position, Vector3.up, out obHit, 0.6f);
        float obstacleDistance = obHit.distance;
        if (isObstacle)
        {
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -5f;
            }
            controller.height = controller.height - obHit.distance;
            controller.radius = 0.2f;
            body.transform.localScale = new Vector3(body.transform.localScale.x,
                0.5f,
                0.5f);
            yield return null;
        }
        else if(!Input.GetKey(KeyCode.C) && !isObstacle)
        {
            controller.height = 2f;
            controller.radius = 0.5f;
            body.transform.localScale = new Vector3(body.transform.localScale.x,
                1f,
                1f);
            yield return null;
        }
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5f;
        }
        if (Input.GetKey(KeyCode.C) && isGrounded)
        {
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -5f;
            }
            controller.height = 0.01f;
            controller.radius = 0.2f;
            body.transform.localScale = new Vector3(body.transform.localScale.x,
                0.5f,
                0.5f);
            yield return null;
        }
        yield return null;
    }

    IEnumerator SetFieldOfView()
    {
        if (velocity.y < -4f)//Field of view
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 120f, Time.deltaTime / 2f);
        }
        yield return null;
    }

    IEnumerator CalculatePlayerMoving(float xPlayer, float zPlayer)
    {
        Vector3 move = transform.right * xPlayer + transform.forward * zPlayer;
        controller.Move(move * playerSpeed * Time.deltaTime);
        velocity.y += (gravity * Time.deltaTime) / 2f;
        controller.Move(velocity * Time.deltaTime);
        yield return null;
    }

    IEnumerator SpeedDecreasingInJumping()
    {
        if (isGrounded == false)//Замедление в прыжке
            playerSpeed = Mathf.Lerp(playerSpeed, 0f, Time.deltaTime * 1.5f);
        yield return null;
    }

    IEnumerator CameraCollisionDetector()//Движение камеры в центр игрока, если она сталкивается с другим объектом//для игры от 3-го лица
    {
        if (Physics.Linecast(transform.position, transform.position + transform.localRotation * camera_offset, out hit))
        {
            cameraObject.localPosition = new Vector3(
                -Vector3.Distance(transform.position, hit.point),//Stop camera from going trough walls
                0.497f,
                -Vector3.Distance(transform.position, hit.point));
            yield return null;
        }
        else
        {
            cameraObject.localPosition = camera_offset;
            yield return null;
        }
        yield return null;
    }
}
