﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float moveSpeed;
	private float moveVelocity;
	public float jumpHeight;
	private bool doubleJumped = false;

	public Transform groundCheck1;
	public Transform groundCheck2;
	public LayerMask whatIsGround;
	private bool grounded = false;


	public Transform firePoint;
	public GameObject memoryBlast;
	public float shotDelay;
	private float shotDelayCounter;
	public int playerDamageOnShot;

	public GameObject punchObject;

	public int jumpDamageToGive;

	private float knockbackCount;
	private Vector2 knockbackAmount;
	private bool underKnockback = false;

	private Animator anim;
	private Rigidbody2D rb2d;

	//Ability fields
	public bool canDoubleJump;
	public bool canShoot;
	public bool canPunch;

	//Sound fields
	//public AudioClip jumpSound;
	//public AudioClip punchSound;

	public AudioSource jumpAudioSource;
	public AudioSource punchAudioSource;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		rb2d = GetComponent<Rigidbody2D> ();

		canDoubleJump = PlayerPrefs.GetInt ("CanDoubleJump") == 1;
		canShoot = PlayerPrefs.GetInt ("CanShoot") == 1;
		canPunch = PlayerPrefs.GetInt ("CanPunch") == 1;

		//PlayerPrefs.SetInt ("CanDoubleJump", canDoubleJump);
		//PlayerPrefs.SetInt ("CanPunch", canPunch);
		//PlayerPrefs.SetInt ("CanShoot", canShoot);
	}

	void FixedUpdate(){
			grounded = Physics2D.Linecast (transform.position, groundCheck1.position, whatIsGround) || Physics2D.Linecast (transform.position, groundCheck2.position, whatIsGround);
		if (grounded)
			doubleJumped = false;
	}

	// Update is called once per frame
	void Update () {
		checkMovement ();
		if (!underKnockback) {
			checkJump ();
			if (canShoot)
				checkShooting ();
			if (canPunch)
				checkPunch ();
		}
		//anim.SetBool ("underKnockback", underKnockback);
		checkFlip ();

		canDoubleJump = PlayerPrefs.GetInt ("CanDoubleJump") == 1;
		canShoot = PlayerPrefs.GetInt ("CanShoot") == 1;
		canPunch = PlayerPrefs.GetInt ("CanPunch") == 1;
	}

	private void checkPunch(){
		if (Input.GetButtonDown ("Fire2")) {
			anim.SetTrigger ("Punch");
			punchAudioSource.Play ();
			//AudioSource.PlayClipAtPoint(punchSound, transform.position);
		}
	}

	private void checkShooting(){
		//Check if the player is shooting, the animation will call the creation of the memory blast
		//Instantiate (memoryBlast, firePoint.position, firePoint.rotation);
		if (Input.GetButtonDown ("Fire1")) {
			anim.SetTrigger ("Shoot");
			shotDelayCounter = shotDelay;
		}

		if (Input.GetButton ("Fire1")) {
			shotDelayCounter -= Time.deltaTime;
			if (shotDelayCounter < 0) {
				anim.SetTrigger ("Shoot");
				shotDelayCounter = shotDelay;
			}
		}
	}

	public void fireMemoryBlast(){
		GetComponent<HealthManager>().HurtPlayer (playerDamageOnShot);
		Instantiate (memoryBlast, firePoint.position, firePoint.rotation);
	}

	private void checkMovement (){
		if (underKnockback = (knockbackCount > 0)) {
			rb2d.velocity = knockbackAmount;
			knockbackCount -= Time.deltaTime;
		} else {
			rb2d.velocity = new Vector2 (moveSpeed * Input.GetAxisRaw("Horizontal"), rb2d.velocity.y);
			anim.SetFloat ("Speed", Mathf.Abs(Input.GetAxisRaw("Horizontal")));
		}
		anim.SetBool ("knockedback", underKnockback);
	}

	public void applyKnockback(Vector2 amount, float time){
		knockbackAmount = amount;
		knockbackCount = time;
		underKnockback = true;
		anim.SetBool ("knockedback", underKnockback);
	}

	private void checkJump(){
		if (grounded) {
			doubleJumped = false;
		}

		if (Input.GetButtonDown ("Jump") && grounded) {
			jump ();
		}

		if (Input.GetButtonDown ("Jump") && !grounded && !doubleJumped && canDoubleJump) {
			jump ();
			doubleJumped = true;
		}
	}

	public void jump(){
		anim.SetTrigger ("Jump");
		rb2d.velocity = new Vector2 (rb2d.velocity.x, jumpHeight);
		jumpAudioSource.Play ();
		//AudioSource.PlayClipAtPoint(jumpSound, transform.position);
	}

	private void checkFlip(){
		//Check if the players sprite is facing the right way
		if (underKnockback) {
			if (rb2d.velocity.x > 0) {
				transform.localScale = new Vector3 (-1, 1, 1);
			} else if (rb2d.velocity.x < 0) {
				transform.localScale = new Vector3 (1, 1, 1);
			}
		} else {
			if (rb2d.velocity.x > 0) {
				transform.localScale = new Vector3 (1, 1, 1);
			} else if (rb2d.velocity.x < 0) {
				transform.localScale = new Vector3 (-1, 1, 1);
			}
		}
	}

	public void respawnPlayer(){
		underKnockback = false;
		knockbackCount = 0;
		doubleJumped = true;
		GetComponent<HealthManager> ().fullHealth ();
	}


	void OnCollisionEnter2D(Collision2D other){
		if (other.transform.tag == "MovingPlatform") {
			transform.parent = other.transform;
		}
	}

	void OnCollisionExit2D(Collision2D other){
		if (other.transform.tag == "MovingPlatform") {
			transform.parent = null;
		}
	}

	public void stopMoving(){
		rb2d.velocity = new Vector3 (0,0,0);
	}
}
