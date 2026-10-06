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
