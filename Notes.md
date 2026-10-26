# General Notes
## Camera state sequence
1) When it's the player's turn to shoot, zoom out to show both the ball and the hole for a short time.
2) Then focus on the ball for aiming and launching.
3) When the player's turn ends, zoom out to show both the ball and the hole.
4) Next player's turn, so translates to show their ball and the hole. Goes back to (2).

## Player turn states
Player's control is removed turn ends when the ball touches the ground.\
Player's turn ends when the ball stops moving.

## Shot indicator
Shot indicator could be a rectangle with the sprite determining the visual shape.
* Pros: Easy to add new shapes.
* Cons: seemingly less control over manipulating shape features (arrowhead point widths, arrow indicator portion max length).

## Database
Amazon RDS for MySQL database hosting.
* $200 in free credits for completing certifications, $100 for starting (I think).

---

# Meeting Notes
## 10/05/2026
* Game is a sidescroller, not top-down.
* Core ground types are regular ground, sand pits and water.
* Camera operates as described in the camera state sequence section.
* Game is turn based.
* Player can click/touch anywhere on the screen to begin aiming.
* Aiming while mid-air slows down time.
* Multiple sections on kanban board, one for each week.

## 10/06/2026
* MySQL suits database needs better.
* Until further in development, everyone should download a MySQL server and set up a testing database.
* Single sql script for table setup.
* GameMaster object will also act as sql client and make calls to the server.
* How do we not leak server credentials in the code? Environment variables?
* Hoping that free $100/$200 from AWS credits let us use Amazon RDS to host the MySQL database for long enough.
* Ground can be shaped with spline component or something like that. EdgeCollider component allows its collision to account for any shape.

## 10/19/2026 
* Role Changes: Elijah will now serve as the QA Tester, and I will take over the role of Project Manager.

* Week 8 Task Review: Two tasks currently under “Waiting for Merge” for Week 8 do not have pull requests attached. We also noticed that several design tasks remain unclaimed or incomplete.

* Beginner Map Priority: The designs for the Beginner Map need to be completed by the end of the week. Completing these designs is important before moving on to the Intermediate Map because the team needs a solid visual and structural foundation before implementing additional game features.

* Team Roles and Bi-Weekly Reports: We reviewed the responsibilities associated with each team role and discussed the specific information that each role is responsible for including in the bi-weekly reports.

## 10/26/2026

## Weekly Standup Formatting

**Formatting:**  
Denny requires the Weekly Standup to follow the exact format and template provided. To avoid losing points, please do not change the font, remove underlines, or modify the required formatting. Make sure all information is organized and spaced evenly for readability.

**Role Switching:**  
Two team roles have officially changed. For the next Weekly Standup, please list:
- **Seth – Project Manager**
- **Elijah – QA Tester**

**Pull Requests:**  
A pull request is required for completed **Regular** tasks. If the correct pull request link is not attached to the task, the task will not count toward the required task points.

Example of a **Pull Request link:**  
https://github.com/CSC-3380-Fall-2026/Team-1/pull/38

Example of an **Issue link, not a Pull Request:**  
https://github.com/CSC-3380-Fall-2026/Team-1/issues/86

---

## Tasks

**Pull Request Requirement:**  
Any completed task that involves changes to the project's code must have its own pull request. The pull request allows the team to review what was implemented or changed and ensures everyone can update their local code accordingly.

Tasks placed in **Waiting For Merge** without a pull request will be moved back to **In Progress** until a pull request is created and approved by the Git Master.

**Task Conflicts:**  
If you are unable to complete an assigned task for any reason, you may unassign yourself so another team member can take responsibility for completing it.

---

## Map Implementation

**Beginner Map:**  
Last week, we focused on implementing new features and fixing issues involving the golf ball and scoring system. This week, one of our main priorities is creating a playable **Beginner Map**.

If you have an idea for the map layout, please share it in Discord. Images, sketches, or screenshots of possible layouts are encouraged.

Creating the maps will also help us determine what additional features are needed and whether any existing features should be changed or removed to improve gameplay.

**Map Designing:**  
If you need help designing the map, including adding shapes, creating curved sprites, or working with other map elements, please reach out to **Tega**.

**Wind Zones:** 

As presented in the project, we also plan to implement wind zones into the game, which will affect the directon of the golf ball when it is hit or airborne. Any ideas about this feature is appreciated.

---

## Camera

**Camera Movement:**  
We plan to have the camera follow and focus on the golf ball as it moves throughout the map, including while the ball is airborne or rolling on the ground.

If you have ideas for how the camera should follow the ball or behave during gameplay, please share them in Discord or discuss them with the team in person. All ideas are welcome.

**Zoom In/Out:**  
We also plan to allow players to zoom the camera in and out so they can see more or less of the map when needed.

If you have ideas for how the zoom feature should work or feel during gameplay, please share them with the team.
