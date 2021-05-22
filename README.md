# Game-Engines-Space-Animation
<h1>Made by Tomasz Galka - C18740411 - tommy.galk@gmail.com</h1>
A mostly code-generated space animation as my Game Engines 2 assignment.

<h1>NOTE</h1>
<h1>Please do not use any of these assets without permission <3</h1>

<h1>Short Description</h1>
<p>This is a space battle simulation where cat girls fight against evil rabbits as said in the storyboard.</p>
<p>Scene begins with Commander Nyox telling Nyami (protagonist) about an incoming fleet of Usakis and giving her orders to neutralize the fleet. She boards a NyanFighter and takes off towards the group of evil Usakis along with the fleet she leads. After she and her fleet go into battle against a large fleet of Usakis. After some time, Nyami receives a distress call from Commander Nyox telling her about an incoming BakuUsa Missile heading straight for the Mothership. In the final scene, the BakuUsa impacts with the Mothership and a large explosion and fire can be seen and heard from the Mothership. The scene ends with Nyami notifying her fleet about what has occurred and issuing a return command. The cutscene ends with the credits rolling.

<h1>Features</h1>
<ul>
<li>Flocking Behaviour</li>
<li>Path Following</li>
<li>Custom AI with Behaviour Trees</li>
<li>Projectile Physics</li>
<li>Animations</li>
<li>Music</li>
<li>Sound Effects</li>
<li>Particle Effects</li>
<li>Credits</li>
<li>Japanese Puns</li>
<li>Cute Cat School Girl</li>
<li>Cutscenes</li>
</ul>

<h1>Behaviours Used</h1>
<ul>
 <li>Flocking Behaviour</li>
 <li>Path Following Behaviour</li>
 <li>Targetting Behaviour</li>
 <li>Collision Avoidance</li>
 <li>Collision Detection</li>
</ul>
<p>A behaviour tree was created using the <i>IBehaviour<i> interface which includes multiple behaviours like <b>Idle, Flock and Targetting</b>. Other behaviours are present as well but were not used. The <i>ShipAI</i> class implements this interface. <i>ShipAI</i> is the main class which controls both the NyanFighters and the Usakis.

<h1>Classes written by me</h1>
<p><b>All classes were written by me. These include: </b></p>
<ul>
 <li>CameraRail.cs (Unused)</li>
 <li>Door.cs (Unused)</li>
 <li>IBehaviour.cs</li>
 <li>IInteractable.cs (Unused)</li>
 <li>Rail.cs (Unused)</li>
 <li>ShipAI.cs</li>
 <li>Utilities.cs</li>
 <li>Choice.cs</li>
 <li>Node.cs</li>
 <li>NodeJoint.cs</li>
 <li>VisualNovel.cs</li>
 <li>VisualNovelPanel.cs</li>
 <li>NodeRenderer.cs</li>
 <li>VisualNovelEditor.cs</li>
 <li>VisualNovelInspector.cs</li>
 <li>VisualNovelPanelEditor.cs</li>
 <li>CameraController.cs</li>
 <li>CreditsController.cs</li>
 <li>CutsceneManager.cs</li>
 <li>Fader.cs</li>
 <li>NyamiController.cs</li>
 <li>Projectile.cs</li>
 <li>SoundSystem.cs</li>
 <li>Vibrator.cs (Unused)</li>
 <li>WorldManager.cs</li>
</ul>

<b>Assets made by me</b>
<p>All 3D assets were made by me. These include: </p>
<ul>
 <li>Nyami Character Model (Hand-Painted, Rigged and Skinned)</li>
 <li>Mothership (Hand-painted and Rigged)</li>
 <li>NyanFighter (Hand-painted)</li>
 <li>Usaki (Hand-painted, Rigged and Skinned)</li>
 <li>BakuUsa</li>
</ul>

<h1>Graphical Techniques Used</h1>
<p>Not many graphical techniques were used, however Particle Effects were used for explosions and Skybox. Trail renderers were used for the Usakis, NyanFighters and BakuUsas (Rockets). The Skybox was achieved by creating a camera which only renders a single particle effect, assigning a render texture to that camera and creating and assigning a skybox material of that render texture. This gave the scene a space-like look without using external skybox assets :)</p>
<b>Post-Processing was not used due to the cartoon-like style I went for.</b>

<h1>Shaders</h1>
<p>Only two shaders were used, one is a two-sided version of the Standard shader I edited and the other is a flat lighting Shader which is a modification of Unity's WrapLambert shader. The files are: 
<ul>
 <li>TestToonShader.shader</li>
 <li>UnlitTwoSided.shader</li>
</ul>
 
<h1>""Gameplay"" Video</h1>
<a href="https://youtu.be/Qk2j1F5Qodk"><img src="http://img.youtube.com/vi/Qk2j1F5Qodk/0.jpg" title="Game Engines 2 - Space Battle Assignment"/></a>

<h1>Storyboard</h1>
<img src="https://raw.githubusercontent.com/Gomystalka/Game-Engines-Space-Animation/main/Cat%20Game%20Storyboard%20Finished.png">

<h1>Credits</h1>
<p>Credit to BloodPixel at FreeSound for Background Music</p>
<p>Credit to Tom McCann at FreeSound for the Explosion sound</p>
<p>Mixamo for walking and idle animations</p>

<p><b>All other assets including character and prop models and Visual Novel Framework were completely made by me. I can provide source files for these assets if required to prove my ownership!</b></p>

<h1>Assets</h1>
<h2>Mothership</h2>
<img src="https://imgur.com/9FdqwNj.png">

<h2>Usaki</h2>
<img src="https://imgur.com/o83po1Z.png">

<h2>NyanFighter</h2>
<img src="https://imgur.com/Mr1ANSz.png">

<h2>Nyami</h2>
<img src="https://imgur.com/MsU5neU.png">
