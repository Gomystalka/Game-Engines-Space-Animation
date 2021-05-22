using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;

    [Header("Settings")]
    public float radius = 10f;
    public Vector3 centerOffset;
    public Vector2 usakiSpeedRange;
    public Fader fader;

    public static float RandomShipSpeed => Random.Range(instance.usakiSpeedRange.x, instance.usakiSpeedRange.y);
    public float maxUsakiSpeed = 2500f;
    public float minUsakiSpeed = 800f;
    public float usakiDecelerationRate = 10f;
    public float bakuHatsuSpeed = 50f;

    public Projectile laserPrefab;
    private VisualNovel _selectedNovel;

    [Header("Stages")]
    public Transform bakuhatsuView;
    public Transform pointOfImpact;
    public VisualNovel finalDialogue;
    public CreditsController creditsController;

    [Header("FX")]
    public GameObject bakuhatsu;
    public GameObject fire;

    public static byte Stage { get; set; } = 0;
    public byte CurrentStage { get => Stage; set => Stage = value; }

    private void Awake()
    {
        instance = Utilities.CreateSingleton(instance, this);
    }

    public static Vector3 QueryRandomWorldPoint() {
        if (!instance) return Vector3.zero;
        return instance.transform.position + instance.centerOffset + (Random.insideUnitSphere * instance.radius);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 1f, 0.4f);
        Gizmos.DrawSphere(transform.position + centerOffset, radius);
    }

    public void FireRandosmBakuUsaAt()
    {
        Debug.Log($"E? STAGE {Stage}");
        if (Stage == 1) {
            CameraController.instance.SetCameraSpeeds(100f, 60f);
            CameraController.instance.SetCameraTarget(bakuhatsuView);
            Transform pr = ShipAI.FireRandomBakuUsaAt(pointOfImpact);
            pr.Inflate(4.5f);
            Projectile proj = pr.GetComponent<Projectile>();
            proj.onHomeTargetReached.AddListener(() => OnBakuUsaHit(proj));
            Stage++;
        }

        //StartCoroutine(PerformAfter(6f, transform));
    }

    private void OnBakuUsaHit(Projectile proj) {
        Destroy(proj.gameObject);
        Instantiate(bakuhatsu, proj.transform.position, Quaternion.identity);
        Instantiate(fire, proj.transform.position, Quaternion.identity);
        SoundSystem.PlaySound(SoundSystem.FindClipByName("Bakuhatsu"), 1, false, 0f);
        Stage++;
        StartCoroutine(LoadNovelAfterSeconds(finalDialogue, 4f));
    }

    public void SetNovelToLoad(VisualNovel novel) => _selectedNovel = novel;
    public void LoadSelectedNovelAfter(float seconds) {
        if (VisualNovelPanel.instance)
            StartCoroutine(LoadNovelAfterSeconds(_selectedNovel, seconds));
    }

    private IEnumerator LoadNovelAfterSeconds(VisualNovel novel, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (novel)
        {
            VisualNovelPanel.instance.gameObject.SetActive(true);
            VisualNovelPanel.instance.LoadVisualNovel(novel);
        }
    }

    public void TryEndCutscene() {
        if (Stage > 2) {
            fader.FadeIn();
        }
    }

    public void RollCredits() => creditsController.RollCredits();

    public void IncrementStage() => Stage++;
}
