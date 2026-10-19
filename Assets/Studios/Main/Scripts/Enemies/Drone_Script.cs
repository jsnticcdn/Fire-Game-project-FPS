using UnityEngine;

public class Drone_Script : MonoBehaviour
{
    // drag these in
    public Transform aimSphere1;
    public Transform aimSphere2;
    public GameObject bulletPrefab;

    // vision (sees all around, walls block it)
    public float viewRange = 25f;

    // roaming
    public float wanderRadius = 10f;
    public float wanderHeight = 3f;
    public float flySpeed = 3f;
    public float attackFlySpeed = 2f;
    public float wanderTimeout = 6f;
    public float turnSpeed = 90f;

    // looking around
    public float lookAroundTime = 4f;
    public float lookAngle = 100f;
    public float lookSpeed = 1.5f;

    // attacking
    public float lockOnTime = 1f;
    public float laserTime = 1f;
    public int shotsInBurst = 4;
    public float timeBetweenShots = 0.2f;
    public float bulletSpeed = 25f;
    public float cooldownTime = 2f;
    public float loseSightTime = 1f;
    public float aimHeight = 1f;

    // laser look
    public Color laserColor = Color.red;
    public float laserWidth = 0.05f;

    Rigidbody rb;
    LineRenderer laser;
    Transform player;
    Vector3 startPos;
    Vector3 wanderTarget;
    Vector3 lastSeenPoint;
    Vector3 lockedPoint;
    bool needNewPoint = true;
    bool lookingAround = false;

    // 0 = roaming, 1 = locking on, 2 = laser showing, 3 = shooting, 4 = cooldown
    int state = 0;
    float timer = 0f;
    float lostTimer = 0f;
    float wanderTimer = 0f;
    float lookTimer = 0f;
    float lookBaseYaw = 0f;
    int shotsFired = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;

        startPos = transform.position;
        lastSeenPoint = startPos;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }

        // makes its own laser, nothing to set up (uses one already on the drone if there is one)
        laser = GetComponent<LineRenderer>();
        if (laser == null)
        {
            laser = gameObject.AddComponent<LineRenderer>();
        }
        laser.positionCount = 2;
        laser.useWorldSpace = true;
        laser.startWidth = laserWidth;
        laser.endWidth = laserWidth;
        laser.material = new Material(Shader.Find("Sprites/Default"));
        laser.startColor = laserColor;
        laser.endColor = laserColor;
        laser.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 playerPoint = player.position + Vector3.up * aimHeight;
        Vector3 toPlayer = playerPoint - transform.position;
        float dist = toPlayer.magnitude;
        Vector3 laserStart = (aimSphere1.position + aimSphere2.position) / 2f;

        // can the drone see the player? (range, then walls)
        bool canSee = false;
        if (dist <= viewRange)
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, toPlayer.normalized, dist, ~0, QueryTriggerInteraction.Ignore);
            float closest = 9999f;
            bool playerIsFirst = false;
            for (int i = 0; i < hits.Length; i++)
            {
                // skip the drone's own colliders
                if (hits[i].transform.root == transform.root) continue;

                if (hits[i].distance < closest)
                {
                    closest = hits[i].distance;
                    playerIsFirst = hits[i].collider.CompareTag("Player");
                }
            }
            canSee = playerIsFirst;
        }

        if (canSee)
        {
            lastSeenPoint = playerPoint;
        }

        // ---------- lost the player while attacking ----------
        if (state != 0)
        {
            if (canSee)
            {
                lostTimer = 0f;
            }
            else
            {
                lostTimer += Time.deltaTime;
            }

            if (lostTimer >= loseSightTime)
            {
                // stop attacking, fly to where it last saw you and look around
                state = 0;
                laser.enabled = false;
                lookingAround = false;
                wanderTarget = lastSeenPoint;
                needNewPoint = false;
                wanderTimer = 0f;
            }
        }

        // ---------- roaming (happens in every state, even while shooting) ----------
        float speed = flySpeed;
        if (state != 0)
        {
            speed = attackFlySpeed;
        }

        bool hovering = false;
        if (state == 0 && lookingAround)
        {
            hovering = true;
        }

        if (needNewPoint)
        {
            // pick a random spot around where the drone started
            for (int tries = 0; tries < 10; tries++)
            {
                Vector3 offset = new Vector3(Random.Range(-wanderRadius, wanderRadius), Random.Range(-wanderHeight, wanderHeight), Random.Range(-wanderRadius, wanderRadius));
                wanderTarget = startPos + offset;

                // check there's no wall between us and that spot
                Vector3 dir = wanderTarget - transform.position;
                RaycastHit[] pathHits = Physics.RaycastAll(transform.position, dir.normalized, dir.magnitude, ~0, QueryTriggerInteraction.Ignore);
                bool blocked = false;
                for (int j = 0; j < pathHits.Length; j++)
                {
                    if (pathHits[j].transform.root != transform.root)
                    {
                        blocked = true;
                    }
                }
                if (!blocked) break;
            }
            needNewPoint = false;
            wanderTimer = 0f;
        }

        Vector3 toTarget = wanderTarget - transform.position;

        if (hovering)
        {
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            rb.linearVelocity = toTarget.normalized * speed;

            wanderTimer += Time.deltaTime;
            if (toTarget.magnitude < 0.5f || wanderTimer >= wanderTimeout)
            {
                if (state == 0)
                {
                    // got there, stop and look around
                    lookingAround = true;
                    lookTimer = 0f;
                    lookBaseYaw = transform.eulerAngles.y;
                }
                else
                {
                    // attacking, so just keep moving to a new spot
                    needNewPoint = true;
                }
            }
        }

        // ---------- states ----------
        if (state == 0)
        {
            if (lookingAround)
            {
                // sweep left and right
                lookTimer += Time.deltaTime;
                float sweep = Mathf.Sin(lookTimer * lookSpeed) * lookAngle;
                transform.rotation = Quaternion.Euler(0f, lookBaseYaw + sweep, 0f);

                if (lookTimer >= lookAroundTime)
                {
                    lookingAround = false;
                    needNewPoint = true;
                }
            }
            else
            {
                // face the way it's flying (kept level so it doesn't tilt)
                Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
                if (flat.magnitude > 0.1f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(flat);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, turnSpeed * Time.deltaTime);
                }
            }

            if (canSee)
            {
                state = 1;
                timer = 0f;
                lostTimer = 0f;
                lookingAround = false;
                needNewPoint = true;
            }
        }
        else
        {
            // every attack state: face the player while moving around
            Quaternion faceRot = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, faceRot, turnSpeed * Time.deltaTime);

            if (state == 1)
            {
                // locking on (no laser yet)
                timer += Time.deltaTime;
                if (timer >= lockOnTime)
                {
                    // locked on to where you are right now, laser shows up
                    state = 2;
                    timer = 0f;
                    lockedPoint = playerPoint;
                    laser.enabled = true;
                }
            }
            else if (state == 2)
            {
                // laser is showing at the locked spot, then it starts shooting
                laser.SetPosition(0, laserStart);
                laser.SetPosition(1, lockedPoint);

                timer += Time.deltaTime;
                if (timer >= laserTime)
                {
                    state = 3;
                    timer = timeBetweenShots;
                    shotsFired = 0;
                }
            }
            else if (state == 3)
            {
                // fire the burst, only while it can actually see you
                laser.SetPosition(0, laserStart);
                laser.SetPosition(1, lockedPoint);

                timer += Time.deltaTime;
                if (timer >= timeBetweenShots && canSee)
                {
                    timer = 0f;

                    Transform muzzle = aimSphere1;
                    if (shotsFired % 2 == 1)
                    {
                        muzzle = aimSphere2;
                    }

                    Quaternion bulletRot = Quaternion.LookRotation(lockedPoint - muzzle.position);
                    GameObject bullet = Instantiate(bulletPrefab, muzzle.position, bulletRot);

                    // make sure the bullet flies straight and doesn't fall
                    Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                    if (bulletRb == null)
                    {
                        bulletRb = bullet.AddComponent<Rigidbody>();
                    }
                    bulletRb.useGravity = false;
                    bulletRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    bulletRb.linearVelocity = bullet.transform.forward * bulletSpeed;

                    shotsFired++;

                    if (shotsFired >= shotsInBurst)
                    {
                        state = 4;
                        timer = 0f;
                        laser.enabled = false;
                    }
                }
            }
            else if (state == 4)
            {
                // short break after shooting (still moving)
                timer += Time.deltaTime;
                if (timer >= cooldownTime)
                {
                    timer = 0f;
                    if (canSee)
                    {
                        state = 1;
                        lostTimer = 0f;
                    }
                    else
                    {
                        // can't see you, go check where you were
                        state = 0;
                        lookingAround = false;
                        wanderTarget = lastSeenPoint;
                        needNewPoint = false;
                        wanderTimer = 0f;
                    }
                }
            }
        }
    }
}

