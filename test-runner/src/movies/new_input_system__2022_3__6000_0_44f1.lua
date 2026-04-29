MOVIE_CONFIG = {
  fps = 100
}

f = movie.frame_advance
hold = key.hold
rel = key.release

-- MousePosLock
mouse.move(155, 205)
f()
mouse.move(140, 140)
f()
mouse.move(550, 550)
f()
mouse.move(550, 550)
f()
f()
f()
mouse.move(75, 25)
f()
mouse.move(-550, -550)
f()
mouse.move(-550, -550)
f()
f()
f()
f()
mouse.move(75, 75)
f()
mouse.move(100, 100)
f()
f()
mouse.move(-100, -100)
f()
f()

-- KeyboardTwoKeys
-- press A and B, but B is 1f delayed in action
f()
hold("A")
f()
hold("B")
f()
rel("A")
f()
rel("B")
f()
f()

-- tap A and B twice, but B is delayed 1f in action
hold("A")
f()
rel("A")
hold("B")
f()
hold("A")
rel("B")
f()
rel("A")
hold("B")
f()
rel("B")
f()
f()
