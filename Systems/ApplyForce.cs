using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using System;

public class ApplyForce : FSystem {
	// Use this to update member variables when system pause. 
	// Advice: avoid to update your families inside this function.
	private Family circleFF = FamilyManager.getFamily(new AllOfComponents(typeof(ForceField), typeof(TriggerSensitive3D)));

    private float minDist = 1;

	protected override void onPause(int currentFrame) {
	}

	// Use this to update member variables when system resume.
	// Advice: avoid to update your families inside this function.
	protected override void onResume(int currentFrame){
	}

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        foreach (GameObject ff in circleFF){
			Triggered3D trig = ff.GetComponent<Triggered3D> ();
			if (trig != null) {
				foreach (GameObject go in trig.Targets) {
					Move mv = go.GetComponent<Move> ();
					if (ff.GetComponent<ForceField> ().ffType == 0 || ff.GetComponent<ForceField> ().ffType == 1) {//if attractive or repulsive
						//CircleEuler(ff, go, mv, 1000);
						CircleVerlet (ff, go, mv, 1000);
					} else if (ff.GetComponent<ForceField> ().ffType == 2) {//if uniform
						Uniform (ff, go, mv, 1000);
					}
				}
			}
		}
	}

	void CircleEuler(GameObject ff, GameObject go, Move mv,int n){
		Vector3 newPoint = go.transform.position;
		for(int i = 0; i <n; i++){ //new position calculated every deltaTime/n
			Vector3 vect = ff.transform.position - newPoint;
			float distance = vect.magnitude;
			//float value = (ff.GetComponent<Mass>().mass*go.GetComponent<Mass>().mass*ff.GetComponent<CircleField>().g)/(distance*distance*1000*1000);
			float value;
			if (distance < minDist) {//if the object is to close from the center 1/r² = 1/minDist²
				value = ff.GetComponent<ForceField> ().value / (minDist * minDist);
			} else {
				value = ff.GetComponent<ForceField> ().value / (distance * distance);//since mass and charge are constant we just pu value/r² (has to be changed)
			}
			if (ff.GetComponent<ForceField> ().ffType == 1) {//for repulsive force fields
				value = -value;
			}
			newPoint+= vect.normalized * value * Time.deltaTime/n;//Euler
		}
        //store values in the move component of the ball
		mv.direction.Normalize ();
		mv.direction = (newPoint - mv.transform.position) + mv.direction*mv.speed;
		mv.speed = mv.direction.magnitude;
		mv.direction.Normalize ();
	}

	void CircleVerlet(GameObject ff, GameObject go, Move mv, int n){
		mv.direction.Normalize ();
		Vector3 v = mv.direction * mv.speed;
		float dt = Time.deltaTime/n;
		Vector3 point = go.transform.position;
		for (int i = 0; i < n; i++) {//new position calculated every deltaTime/n
			point += v * dt;
			Vector3 vect = ff.transform.position - point;
			float distance = vect.magnitude;
			float value;
			if (distance < minDist) {//if the object is to close from the center 1/r² = 1/minDist²
				value = ff.GetComponent<ForceField> ().value / (minDist * minDist);
			} else {
				value = ff.GetComponent<ForceField> ().value / (distance * distance);
			}
			if (ff.GetComponent<ForceField> ().ffType == 1) {//for repulsive force fields
				value = -value;
			}
            //Verlet Method
			Vector3 newPos = point + v * dt + vect.normalized * value * dt * dt / 2;
			Vector3 vectDT = ff.transform.position - newPos;
			float distanceDT = vectDT.magnitude;
			float valueDT;
			if (distanceDT < 1) {
				valueDT = ff.GetComponent<ForceField> ().value;
			} else {
				valueDT = ff.GetComponent<ForceField> ().value / (distanceDT * distanceDT);
			}
			if (ff.GetComponent<ForceField> ().ffType == 1) {
				valueDT = -valueDT;
			}
			v = v + (vect.normalized * value + vectDT.normalized * valueDT) * dt / 2;
        }
        //store values in the move component of the ball
        mv.direction = v;
		mv.speed = mv.direction.magnitude;
		mv.direction.Normalize ();
	}

	void Uniform(GameObject ff, GameObject go, Move mv,int n){
		Vector3 newPoint = go.transform.position;
		for(int i = 0; i <n; i++){ //new position calculated every deltaTime/n
			float value = ff.GetComponent<ForceField> ().value;
			float alpha = (float)(ff.GetComponent<ForceField> ().direction*Math.PI/180);//direction of the uniform force field
			newPoint+= new Vector3((float)Math.Cos(alpha),0, (float)Math.Sin(alpha)) * value * Time.deltaTime/n;//Euler
        }
        //store values in the move component of the ball
        mv.direction.Normalize ();
		mv.direction = (newPoint - mv.transform.position) + mv.direction*mv.speed;
		mv.speed = mv.direction.magnitude;
		mv.direction.Normalize ();
	}
}