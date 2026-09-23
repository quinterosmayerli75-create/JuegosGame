using System.Collections.Generic;
using UnityEngine;

// =====================================================================
// APRENDIZ
// Un "cerebro" de aprendizaje reutilizable. Implementa un algoritmo de
// bandido multibrazo (multi-armed bandit) con estrategia epsilon-greedy.
//
// Problema que resuelve: hay varias opciones (ej. 15 galletas o
// 5 tamaños) y no sabemos cuál es la mejor. Hay que probarlas y
// aprender de los resultados.
//
// No es un MonoBehaviour: es una clase normal de C#. No va en ningún
// GameObject; la Celula crea dos (uno para galleta, otro para tamaño).
// =====================================================================
public class Aprendiz
{
    // --- Memoria: una entrada por cada opción ---
    private int[] intentos;  // Cuántas rondas se usó cada opción
    private int[] exitos;    // En cuántas de esas rondas sobrevivió
    private bool[] activa;   // false = opción descartada para siempre

    // --- Parámetros de exploración ---
    private float epsilon;            // Probabilidad de explorar (elegir al azar)
    private float epsilonMinimo;
    private float factorDecaimiento;  // Cuánto baja epsilon cada ronda

    // --- Parámetros de descarte ---
    private int intentosMinimos;      // Datos mínimos antes de juzgar una opción
    private float margenDescarte;     // Qué tan peor que la mejor debe ser

    public int OpcionActual { get; private set; }

    public Aprendiz(int cantidadOpciones, float epsilon, float epsilonMinimo,
                    float factorDecaimiento, int intentosMinimos, float margenDescarte)
    {
        intentos = new int[cantidadOpciones];
        exitos = new int[cantidadOpciones];
        activa = new bool[cantidadOpciones];
        for (int i = 0; i < cantidadOpciones; i++) activa[i] = true;

        this.epsilon = epsilon;
        this.epsilonMinimo = epsilonMinimo;
        this.factorDecaimiento = factorDecaimiento;
        this.intentosMinimos = intentosMinimos;
        this.margenDescarte = margenDescarte;

        OpcionActual = Random.Range(0, cantidadOpciones); // Primera ronda: al azar
    }

    // Se llama al final de cada ronda: guarda el resultado de la opción usada,
    // aprende (descarta opciones malas) y elige la opción de la próxima ronda.
    public int RegistrarYElegir(bool sobrevivio)
    {
        // 1) REGISTRAR
        intentos[OpcionActual]++;
        if (sobrevivio) exitos[OpcionActual]++;

        // 2) APRENDER
        epsilon = Mathf.Max(epsilonMinimo, epsilon * factorDecaimiento);
        DescartarMalas();

        // 3) DECIDIR (epsilon-greedy)
        OpcionActual = (Random.value < epsilon) ? AlAzar() : Mejor();
        return OpcionActual;
    }

    // Tasa de supervivencia de una opción (éxitos / intentos).
    // Si nunca se probó vale 1 (inicialización optimista): así el
    // aprendiz se anima a probar todas las opciones al menos una vez.
    public float Valor(int i)
    {
        if (intentos[i] == 0) return 1f;
        return (float)exitos[i] / intentos[i];
    }

    private void DescartarMalas()
    {
        float mejor = MejorValor();
        for (int i = 0; i < activa.Length; i++)
        {
            bool suficientesDatos = intentos[i] >= intentosMinimos;
            bool muchoPeor = Valor(i) < mejor - margenDescarte;
            if (activa[i] && suficientesDatos && muchoPeor && CantidadActivas() > 1)
            {
                activa[i] = false;
            }
        }
    }

    private float MejorValor()
    {
        float mejor = float.MinValue;
        for (int i = 0; i < activa.Length; i++)
            if (activa[i] && Valor(i) > mejor) mejor = Valor(i);
        return mejor;
    }

    // Explotar: la opción con mejor tasa. Si hay empate, una de ellas al azar.
    private int Mejor()
    {
        float mejor = MejorValor();
        List<int> candidatas = new List<int>();
        for (int i = 0; i < activa.Length; i++)
            if (activa[i] && Mathf.Approximately(Valor(i), mejor)) candidatas.Add(i);
        return candidatas.Count > 0 ? candidatas[Random.Range(0, candidatas.Count)] : AlAzar();
    }

    // Explorar: cualquier opción que siga activa.
    private int AlAzar()
    {
        List<int> activas = new List<int>();
        for (int i = 0; i < activa.Length; i++)
            if (activa[i]) activas.Add(i);
        return activas.Count > 0 ? activas[Random.Range(0, activas.Count)] : 0;
    }

    private int CantidadActivas()
    {
        int n = 0;
        foreach (bool a in activa) if (a) n++;
        return n;
    }

    // Texto con la tabla de aprendizaje (útil para depurar y para la documentación)
    public string Resumen()
    {
        string s = "";
        for (int i = 0; i < activa.Length; i++)
        {
            s += "[" + i + "] " + exitos[i] + "/" + intentos[i]
               + (activa[i] ? "" : " (descartada)") + "   ";
        }
        return s;
    }
}