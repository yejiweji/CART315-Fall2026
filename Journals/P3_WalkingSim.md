# Design Journal: Walking Simulator

## Journal Update: #1
Initially, I was thinking of making a squirrel simulator where the player would navigate across electrical wires and the rooftops of tall skyscrapers. I liked the idea of exploring an environment from a different perspective, but the more I thought about it, the more the concept felt like something I'd already seen before. I wasn't sure what would make the experience meaningful beyond the novelty of being a squirrel.

While scrolling through walking simulators on itch.io, I noticed how many had Backrooms-esque environments. This got me thinking less about what the player would physically do and more about what I wanted them to feel. Rather than creating something mechanically engaging or requiring the player to actively solve problems, I wanted to see if I could build an experience around a specific emotion.

One of the most visceral feelings I experience is the panic that comes with trying to make the right decision when I don't have enough information to feel confident in either choice. For example, when I know my bus is coming in two minutes and see it approaching down the street, I have to decide whether to run towards my original bus stop or try to catch it at the next one. I don't know which option gives me a better chance of catching it, but I still have to choose before it's too late. The uncertainty makes the decision feel much more urgent than it probably is. I kept thinking about how that feeling could translate into a walking simulator.

This led me to the idea of an endless apartment stairwell. I imagined a winding staircase with no windows, where the player has to rely on what they can see and hear around them. Light would spill around the bends at both the top and bottom of the stairs, making it seem like an exit is only one or two flights away. However, no matter which direction the player chooses, they never seem to get any closer to leaving.

I want the stairwell to start off relatively normal and well-lit before gradually becoming darker and more ominous. As visibility decreases, it would become harder for the player to understand where they are, while the lights ahead would continue to suggest they're close to getting out. Without windows or a clear view of the outside, they would have no reliable way to orient themselves or determine whether going up or down is the better decision. I want to see if repetitive architecture, limited visibility, and sound can create a feeling of being trapped without needing to explain why the player is there.

What interests me most about this idea is that the player is free to move in either direction, but that freedom doesn't necessarily help them. They can keep making decisions without knowing whether they're getting closer to their goal or making things worse. My next step is to build a basic stairwell in Unity and experiment with its layout and lighting. I want to see how far I can take this uncertainty and whether an environment that keeps promising an exit without ever delivering it can make the player feel the same desperation I experience in those everyday moments of indecision.



## Journal Update: #2

As I started building the stairwell, I wanted to make the space tighter to make it feel more uncomfortable to walk through. I also considered leaving gaps in the structure so the player could see the world outside and question where they were. I even thought about letting them fall out of the world and die. However, I think I want to keep things more realistic for now rather than making the environment too obviously strange.

I'm still not sure how I feel about the exposed underside of the stairs. It gives the space that Backrooms look, but I can't tell if it actually adds to the experience or just makes the model look unfinished. I don't want to throw in weird architectural details just because they make the space look unsettling. I'm also planning to add flickering lights, which I think could make the stairwell feel a bit more unpredictable without having to change too much about the actual structure.

Walking up the stairs hasn't felt as smooth as I'd like, either. I'm not sure if it's a problem with how I built them or something with the player controller. Part of me wonders if the awkward movement could actually make the player feel more uneasy, since even walking through the space doesn't feel completely comfortable. At the same time, I don't want it to come across as a mistake in the build rather than something I intended.

I also considered adding a counter to track how far the player has gone, but I'm worried that would make the idea of panic too obvious. I think I'd rather have the player wonder how long they've been walking and whether they're actually getting anywhere than give them a number to focus on.

I definitely need to experiment with textures more, too. Right now, having everything made of concrete makes the stairwell feel kind of generic. It's giving Roblox more than the liminal space I had in mind. I want it to look like an actual apartment stairwell, just one that feels increasingly wrong the longer you spend in it. I think I'll focus on the materials and lighting next and see how much they change the atmosphere before adding anything else.


## Journal Update: #3

I've started adding lights to the stairwell, using Echelon stairs as inspiration. I added spotlights in certain areas to make the space feel a little off, even though there's nothing immediately threatening the player. I want the stairwell to feel like a normal apartment building at first, but have small details that make the player question whether something is wrong. I also made the lights flicker whenever the anomaly sound plays, so the lighting and audio work together to draw the player's attention to these moments.

I considered using a distinct colour for the lighting to make the stairwell feel more ominous, but I'm worried that would make it too obvious that something bad is going to happen. If the lighting immediately looks unnatural, the player might start anticipating a jumpscare instead of gradually becoming suspicious. I'd rather have them feel uneasy without knowing exactly why. I want them to be caught off guard rather than feel like the game is preparing them for something scary.

I've also added footsteps that match the player's movement, which I think is especially important for a walking simulator. Since most of the experience involves walking through the stairwell, I wanted the player to hear their own footsteps rather than feel like they're just moving a character around a space. It helps make the experience feel more immersive.

For the background audio, I chose a sound with water dripping, echoes, and occasional banging. I liked how it makes the building sound abandoned, but the occasional noises also leave room for the possibility that someone else might be there. I also added a separate sound effect that plays when the lights flicker. It sounds like someone walking on the stairs, but it's difficult to tell whether they're coming from above or below. I wanted to create the feeling that the player might not be alone without actually confirming it. Since they can't tell where the footsteps are coming from, they have to rely on what they hear without being able to fully understand what's happening around them.

Another idea I had was to add an energy bar to show the player getting tired from climbing so many stairs. I thought it could help communicate the exhaustion of being stuck in an endless stairwell, but I'm not sure it fits the experience. A visible bar would give the player something concrete to focus on, whereas I want them to lose track of how long they've been walking and whether they're making progress. I'm leaning towards leaving it out for now and seeing whether the repetitive climbing can create that feeling on its own.

I'm also considering adding details you'd normally find in an apartment stairwell, like railings, floor numbers, and fire exit signs, then gradually removing them as the player continues up or down. I like this idea because those familiar details would help establish the space as somewhere ordinary before things start feeling wrong. If they slowly disappear, the player might notice that the environment is changing without understanding why. I haven't decided whether to include this yet, but I want to experiment with how much I can change the environment while still keeping it believable. So far, I'm focusing on using lighting and sound to make the player question their surroundings rather than relying on obvious horror elements.


## Tooling Disclosure
Stairs:
https://periltek.itch.io/stairs-pack-free 

Light fixture:
https://assetstore.unity.com/packages/3d/props/interior/free-pbr-lamps-70181

Wall texture (concrete):
https://assetstore.unity.com/packages/2d/textures-materials/realistic-concrete-texture-305716

