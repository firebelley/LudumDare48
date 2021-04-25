tool
extends TileMap


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	update_bitmask_region(get_used_rect().position, get_used_rect().end)


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
