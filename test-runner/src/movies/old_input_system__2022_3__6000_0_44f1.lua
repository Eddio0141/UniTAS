MOVIE_CONFIG = {
  fps = 100
}

f = movie.frame_advance

-- MousePosLock
mouse.move(155, 205)
f()
mouse.move(140, 140)
f()
mouse.move(550, 550)
f()
mouse.move(550, 550)
f(3)
mouse.move(75, 25)
f()
mouse.move(-550, -550)
f()
mouse.move(-550, -550)
f(4)
mouse.move(75, 75)
f()
mouse.move(100, 100)
f(2)
mouse.move(-100, -100)
f(2)

-- MouseAxis
mouse.move(150, 142)
f()
mouse.move(50, 32)
f(3)
-- limit within resolution
mouse.move(10000, 10000)
f()
mouse.move(-10000, -10000)
f()
