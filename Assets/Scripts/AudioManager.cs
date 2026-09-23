using UnityEngine;

// =====================================================================
// AUDIO MANAGER
// Controla toda la música y los efectos de sonido del juego.
//
// Usa el patrón "Singleton": solo puede existir UNO en todo el juego.
// Cualquier script puede llamarlo con AudioManager.Instancia.
//
// Con DontDestroyOnLoad sobrevive al cambio de escena, así la música
// sigue sonando sin cortarse al pasar del menú de inicio al juego.
// =====================================================================
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia;

    [Header("Fuentes de audio (los 'parlantes')")]
    public AudioSource fuenteMusica;   // Reproduce la música en loop
    public AudioSource fuenteEfectos;  // Reproduce los efectos cortos

    [Header("Clips de audio (los 'archivos de sonido')")]
    public AudioClip musica;
    public AudioClip sonidoMordida;
    public AudioClip sonidoRonda;
    public AudioClip sonidoBoton;

    [Header("Volumen")]
    [Range(0f, 1f)] public float volumenMusica = 0.4f;
    [Range(0f, 1f)] public float volumenEfectos = 0.8f;

    void Awake()
    {
        // Si ya existe un AudioManager (por ejemplo, venimos del menú),
        // este se destruye para no tener dos músicas sonando a la vez.
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (fuenteMusica != null && musica != null)
        {
            fuenteMusica.clip = musica;
            fuenteMusica.loop = true;
            fuenteMusica.volume = volumenMusica;
            fuenteMusica.Play();
        }
    }

    // Reproduce un efecto. PlayOneShot permite que varios sonidos
    // se superpongan (si muerdes dos galletas rápido, suenan las dos).
    private void Sonar(AudioClip clip, float variacionTono = 0f)
    {
        if (fuenteEfectos == null || clip == null) return;

        // Una pequeña variación de tono evita que el sonido se sienta repetitivo
        fuenteEfectos.pitch = 1f + Random.Range(-variacionTono, variacionTono);
        fuenteEfectos.PlayOneShot(clip, volumenEfectos);
    }

    public void SonarMordida() { Sonar(sonidoMordida, 0.12f); }
    public void SonarRonda() { Sonar(sonidoRonda); }
    public void SonarBoton() { Sonar(sonidoBoton); }
}