using UnityEngine;
using UnityEngine.SceneManagement;

// =====================================================================
// MENU INICIO
// Controla la pantalla de inicio. Tiene una sola función pública,
// Jugar(), que se conecta al evento OnClick del botón en el Inspector.
// =====================================================================
public class MenuInicio : MonoBehaviour
{
    // Nombre exacto de la escena del juego (debe estar en Build Settings)
    public string escenaJuego = "JuegoCamuflaje";

    // Se ejecuta cuando el jugador presiona el botón "Jugar"
    public void Jugar()
    {
        // El sonido sigue aunque cambie la escena, porque el AudioManager
        // no se destruye al cargar la escena nueva.
        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.SonarBoton();
        }

        SceneManager.LoadScene(escenaJuego);
    }
}