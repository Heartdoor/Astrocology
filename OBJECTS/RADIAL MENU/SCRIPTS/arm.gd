@tool
extends Node2D

@onready var animation_player = %AnimationPlayer

func _ready():
	animation_player.play("show")


func _process(delta):
	global_rotation = 0
