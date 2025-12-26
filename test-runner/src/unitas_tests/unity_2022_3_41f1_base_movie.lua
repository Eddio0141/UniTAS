MOVIE_CONFIG = {
  fps = 100
}

f = movie.frame_advance

-- check first input
key.hold("space")
f()
key.release("space")
f()

f(198) -- to let some tests run

-- start test
key.hold("return")
f()
key.release("return")

f(10)
f()
-- scene load
f()

-- keyboard
f()
key.hold("a")
f(2)
key.release("a")
-- TODO: once game resolution is deterministic, set this to center of screen
mouse.move(0, 0) -- prep mouse test
f(2)

-- mouse
-- gui button click
for _ = 1, 5 do
  mouse.left()
  f()
  mouse.left(false)
  f()
end

f()
mouse.move(123, 456)

f()
mouse.left(true)
f(2)
mouse.left(false)
f(2)
