using UnityEngine;

// =====================================================================
// CELULA (galleta)
// Cada galleta es un agente que aprende a sobrevivir a los clics del
// jugador. Toma DOS decisiones cada ronda, cada una con su propio
// "cerebro" (clase Aprendiz, bandido multibrazo epsilon-greedy):
//   1) Qué galleta ser  -> su color / apariencia
//   2) Qué tamaño tener -> entre un mínimo y un máximo
// =====================================================================
public class Celula : MonoBehaviour
{
    [Header("Galletas posibles (color / apariencia)")]
    public Sprite[] galletasPosibles;

    [Header("Tamaños posibles (límites mínimo y máximo)")]
    public float tamanoMinimo = 0.8f;
    public float tamanoMaximo = 2.0f;
    [Range(2, 10)]
    public int cantidadTamanos = 5; // Divide el rango en N opciones

    [Header("Exploración (epsilon-greedy)")]
    [Range(0f, 1f)]
    public float epsilon = 0.3f;            // Probabilidad inicial de explorar
    public float epsilonMinimo = 0.01f;     // Nunca deja de explorar del todo
    [Range(0f, 1f)]
    public float factorDecaimiento = 0.85f; // Cada ronda explora un poco menos

    [Header("Descarte de opciones malas")]
    public int intentosMinimosParaDescartar = 3;
    [Range(0f, 1f)]
    public float margenDescarte = 0.4f;

    // Los dos "cerebros" de la célula
    private Aprendiz aprendizGalleta;
    private Aprendiz aprendizTamano;

    private bool fueDetectada = false;       // ¿El jugador la eliminó esta ronda?
    private bool haHechoPrimeraRonda = false;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        aprendizGalleta = new Aprendiz(galletasPosibles.Length, epsilon, epsilonMinimo,
                                       factorDecaimiento, intentosMinimosParaDescartar, margenDescarte);
        aprendizTamano = new Aprendiz(cantidadTamanos, epsilon, epsilonMinimo,
                                      factorDecaimiento, intentosMinimosParaDescartar, margenDescarte);
        AplicarParametros();
    }

    // Convierte el número de opción (0, 1, 2...) en un tamaño real
    // repartido de forma pareja entre el mínimo y el máximo.
    // Ej: min 0.8, max 2.0, 5 opciones -> 0.8, 1.1, 1.4, 1.7, 2.0
    private float TamanoDeOpcion(int opcion)
    {
        float t = (float)opcion / (cantidadTamanos - 1);
        return Mathf.Lerp(tamanoMinimo, tamanoMaximo, t);
    }

    // Le pone a la célula la galleta y el tamaño que eligieron sus cerebros.
    public void AplicarParametros()
    {
        if (spriteRenderer != null && galletasPosibles.Length > 0)
        {
            spriteRenderer.sprite = galletasPosibles[aprendizGalleta.OpcionActual];
            spriteRenderer.color = Color.white;
        }

        float tamano = TamanoDeOpcion(aprendizTamano.OpcionActual);
        transform.localScale = new Vector3(tamano, tamano, 1f);
    }

    // El GameManager la llama al terminar cada ronda.
    public void NuevaRonda()
    {
        // La primera llamada ocurre al iniciar el juego: aún no hay nada que aprender
        if (haHechoPrimeraRonda)
        {
            bool sobrevivio = !fueDetectada;

            // Los dos cerebros aprenden del MISMO resultado
            aprendizGalleta.RegistrarYElegir(sobrevivio);
            aprendizTamano.RegistrarYElegir(sobrevivio);
        }

        fueDetectada = false;
        haHechoPrimeraRonda = true;

        AplicarParametros();
        gameObject.SetActive(true); // Reaparece aunque la hayan eliminado
    }

    // Tablas de aprendizaje en texto (para mostrarlas en la consola)
    public string ResumenAprendizaje()
    {
        return "Galletas: " + aprendizGalleta.Resumen() + "\nTamaños: " + aprendizTamano.Resumen();
    }

    // El jugador hizo clic: la célula "muere" hasta la próxima ronda.
    void OnMouseDown()
    {
        fueDetectada = true;

        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.SonarMordida();
        }

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.RegistrarEliminacion();
        }

        gameObject.SetActive(false);
    }
}