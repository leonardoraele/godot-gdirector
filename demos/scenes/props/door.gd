extends Node3D

signal open
signal close

func on_open()->void:
	open.emit()

func on_close()->void:
	close.emit()
