using System.Collections.Generic;
using UnityEngine;

public class Celula : MonoBehaviour
{
    [Header("Parámetros de la célula")]
    public float tamanoInicial = 1f;
    public float tamanoMinimo = 0.2f;
    [Range(0.5f, 0.99f)]
    public float factorReduccionTamano = 0.90f; // Se encoge 10% por ronda que sobrevive
    private float tamanoActual;

    [Header("Aprendizaje por Refuerzo")]
    // Lista de colores para mimetismo progresivo
    public Color[] coloresPosibles = new Color[]
    {
        Color.red,
        Color.yellow,
        Color.white,
        new Color(0.10f, 0.60f, 0.80f, 1f), // Celeste (visible pero azulado)
        new Color(0.15f, 0.25f, 0.45f, 1f), // Azul marino
        new Color(0.20f, 0.32f, 0.51f, 1f)  // Tono similar al fondo (#335383)
    };

    [Range(0f, 1f)]
    public float epsilon = 0.5f; // Mayor exploración al inicio
    public float epsilonMinimo = 0.01f;
    [Range(0f, 1f)]
    public float factorDecaimiento = 0.80f;

    [Header("Eliminación de colores malos")]
    public int rondaMinimaAntesDeEliminar = 2;
    public float margenEliminacion = 1f;

    private float[] puntajes;
    private bool[] colorActivo;
    private int colorActualIndice = 0;
    private bool fueDetectada = false;
    private bool haHechoPrimeraRonda = false;
    private int rondaActual = 0;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        tamanoActual = tamanoInicial;

        puntajes = new float[coloresPosibles.Length];
        colorActivo = new bool[coloresPosibles.Length];

        for (int i = 0; i < colorActivo.Length; i++)
        {
            colorActivo[i] = true;
        }
    }

    void Start()
    {
        colorActualIndice = Random.Range(0, coloresPosibles.Length);
        AplicarParametros();
    }

    public void AplicarParametros()
    {
        if (spriteRenderer != null && coloresPosibles.Length > 0)
        {
            spriteRenderer.color = coloresPosibles[colorActualIndice];
        }

        transform.localScale = new Vector3(tamanoActual, tamanoActual, 1f);
    }

    public void NuevaRonda()
    {
        if (haHechoPrimeraRonda && !fueDetectada)
        {
            puntajes[colorActualIndice] += 1f;
            tamanoActual = Mathf.Max(tamanoMinimo, tamanoActual * factorReduccionTamano);
        }

        fueDetectada = false;
        haHechoPrimeraRonda = true;
        rondaActual++;

        epsilon = Mathf.Max(epsilonMinimo, epsilon * factorDecaimiento);

        if (rondaActual >= rondaMinimaAntesDeEliminar)
        {
            EliminarColoresMalos();
        }

        colorActualIndice = ElegirColor();
        AplicarParametros();
        gameObject.SetActive(true);
    }

    private void EliminarColoresMalos()
    {
        float mejor = MejorPuntajeActivo();

        for (int i = 0; i < puntajes.Length; i++)
        {
            if (colorActivo[i] && puntajes[i] < mejor - margenEliminacion)
            {
                colorActivo[i] = false;
            }
        }
    }

    private float MejorPuntajeActivo()
    {
        float mejor = float.MinValue;
        for (int i = 0; i < puntajes.Length; i++)
        {
            if (colorActivo[i] && puntajes[i] > mejor)
            {
                mejor = puntajes[i];
            }
        }
        return mejor;
    }

    private int ElegirColor()
    {
        if (Random.value < epsilon)
        {
            return ColorActivoAlAzar();
        }

        return MejorColorActivo();
    }

    private int MejorColorActivo()
    {
        float mejor = MejorPuntajeActivo();

        List<int> candidatos = new List<int>();
        for (int i = 0; i < puntajes.Length; i++)
        {
            if (colorActivo[i] && puntajes[i] == mejor)
            {
                candidatos.Add(i);
            }
        }

        if (candidatos.Count == 0) return ColorActivoAlAzar();

        return candidatos[Random.Range(0, candidatos.Count)];
    }

    private int ColorActivoAlAzar()
    {
        List<int> activos = new List<int>();
        for (int i = 0; i < colorActivo.Length; i++)
        {
            if (colorActivo[i]) activos.Add(i);
        }

        if (activos.Count == 0) return 0;

        return activos[Random.Range(0, activos.Count)];
    }

    void OnMouseDown()
    {
        fueDetectada = true;
        puntajes[colorActualIndice] -= 1f;

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.RegistrarEliminacion();
        }

        gameObject.SetActive(false);
    }
}