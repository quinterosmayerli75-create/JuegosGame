using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Spawn de células")]
    public Celula prefabCelula;
    public int cantidadCelulas = 5;
    public float limiteX = 4f;
    public float limiteY = 2.5f;

    [Header("Contador de clics")]
    public int celulasEliminadas = 0;

    [Header("Temporizador de rondas")]
    public float duracionRonda = 10f;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoRonda;
    public TextMeshProUGUI textoPuntaje;

    private float tiempoRestante;
    private int numeroRonda = 1;
    private List<Celula> celulas = new List<Celula>();

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        CrearCelulasIniciales();
        IniciarRonda();
    }

    void Update()
    {
        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            SiguienteRonda();
        }

        ActualizarUI();
    }

    // Instancia las células ÚNICAMENTE una vez al iniciar el juego
    void CrearCelulasIniciales()
    {
        for (int i = 0; i < cantidadCelulas; i++)
        {
            Vector3 posicion = ObtenerPosicionAleatoria();
            Celula nueva = Instantiate(prefabCelula, posicion, Quaternion.identity);
            celulas.Add(nueva);
        }
    }

    void IniciarRonda()
    {
        tiempoRestante = duracionRonda;

        // En lugar de destruir, reposicionamos y actualizamos las células existentes
        foreach (Celula c in celulas)
        {
            if (c != null)
            {
                c.transform.position = ObtenerPosicionAleatoria();
                c.NuevaRonda(); // Mantiene su memoria de aprendizaje y tamaño
            }
        }

        ActualizarUI();
    }

    void SiguienteRonda()
    {
        numeroRonda++;
        Debug.Log("Nueva ronda: " + numeroRonda);
        IniciarRonda();
    }

    Vector3 ObtenerPosicionAleatoria()
    {
        return new Vector3(
            Random.Range(-limiteX, limiteX),
            Random.Range(-limiteY, limiteY),
            0f
        );
    }

    void ActualizarUI()
    {
        if (textoTiempo != null)
        {
            textoTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempoRestante).ToString();
        }

        if (textoRonda != null)
        {
            textoRonda.text = "Ronda: " + numeroRonda;
        }

        if (textoPuntaje != null)
        {
            textoPuntaje.text = "Células Eliminadas: " + celulasEliminadas;
        }
    }

    public void RegistrarEliminacion()
    {
        celulasEliminadas++;
        Debug.Log("Células eliminadas: " + celulasEliminadas);
    }
}