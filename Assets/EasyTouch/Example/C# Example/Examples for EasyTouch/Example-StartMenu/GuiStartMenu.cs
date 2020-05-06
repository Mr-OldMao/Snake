using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class GuiStartMenu : MonoBehaviour {
	
	void OnEnable(){	
		EasyTouch.On_SimpleTap += On_SimpleTap;
	}
	
	void OnGUI() {
	            
		GUI.matrix = Matrix4x4.Scale( new Vector3( Screen.width / 1024.0f, Screen.height / 768.0f, 1 ) );
		
		GUI.Box( new Rect( 0, -4, 1024, 70 ), "" );
		
	}
	
	void On_SimpleTap( Gesture gesture){
	
		if ( gesture.pickObject!=null){
			string levelName= gesture.pickObject.name;

			if (levelName == "OneFinger")
				SceneManager.LoadScene("Onefinger"); 
			else if (levelName=="TwoFinger")
				SceneManager.LoadScene("TwoFinger");
			else if (levelName=="MultipleFinger")
				SceneManager.LoadScene("MultipleFingers");	
			else if (levelName=="MultiLayer")
				SceneManager.LoadScene("MultiLayers");
			else if (levelName=="GameController")
				SceneManager.LoadScene("GameController");
			else if (levelName=="FreeCamera")
				SceneManager.LoadScene("FreeCam");			
			else if (levelName=="ImageManipulation")
				SceneManager.LoadScene("ManipulationImage");
			else if (levelName=="Joystick1")
				SceneManager.LoadScene("FirstPerson-DirectMode-DoubleJoystick");		
			else if (levelName=="Joystick2")
				SceneManager.LoadScene("ThirdPerson-DirectEventMode-DoubleJoystick");
			else if (levelName=="Button")
				SceneManager.LoadScene("ButtonExample");			
			else if (levelName=="Exit")
				Application.Quit();						
		}
		
	}
}
