using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
		public float translateSpeed = 0.05f;

		float xTo = 0, zTo = 0;

		// Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float translationX = Input.GetAxis("Vertical") * translateSpeed;
				float translationZ = Input.GetAxis("Horizontal") * translateSpeed;

				xTo = lerp(xTo, translationX, 0.1f);
				zTo = lerp(zTo, translationZ, 0.1f);

				transform.Translate(new Vector3(xTo, 0, zTo));
    }



		Vector3 lerp(Vector3 start, Vector3 end, float speed)
	  {
	    	return (start + ((end - start) * speed));
	  }

		float lerp(float start, float end, float speed)
	  {
	    	return (start + ((end - start) * speed));
	  }

		float clamp(float value, float min, float max)
		{
			  return (Mathf.Max(Mathf.Min(value, max), min));
		}

		float degrees_to_radians(float degrees)
		{
			  float pi = Mathf.PI;
			  return degrees * (pi/180);
		}

		float radians_to_degrees(float radians)
		{
			  float pi = Mathf.PI;
			  return radians * (180/pi);
		}

		float lengthdir_x(float length, float direction)
		{
    		return (Mathf.Cos(degrees_to_radians(direction)) * length); //x
		}

		float lengthdir_y(float length, float direction)
		{
	    	return (Mathf.Sin(degrees_to_radians(direction)) * length); //y
		}
}
