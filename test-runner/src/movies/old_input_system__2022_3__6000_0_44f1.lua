MOVIE_CONFIG = {
  fps = 100
}

f = movie.frame_advance

-- initial
mouse.move(123, 456)
key.hold("a")

-- prepare for next test
f()
key.release("a")
f()

-- keyboard
for _ = 1, 10 do
  key.hold("a")
  f()
  key.release("a")
  f()
end

-- mouse
for _ = 1, 5 do
  mouse.left()
  f()
  mouse.left(false)
  f()
end

for _ = 1, 100 do
  mouse.move_rel(10, 10)
  f()
end

mouse.move(123, 456)
f()

-- repeat mouse movement same to before
for i = 10, 1000, 10 do
  mouse.move(123 + i, i)
  f()
end
