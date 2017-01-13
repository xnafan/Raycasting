using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raycasting;
using Microsoft.Xna.Framework;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        int[,] map = new int[3, 3] { { 1, 1, 1 }, { 1, 0, 1 }, { 1, 1, 1 } };
        
        //TODO: Test for null returns on erroneous slopes (horizontal/vertical)

        [TestMethod]
        public void TestWholeGridCollisionFinderVertically()
        {
            //Arrange
            Player player = new Raycasting.Player(map);
            player.Position = new Vector2(1.5f, 1.5f);
            player.ViewingAngle = 45;
            
            //Act
            var collisionPoint = map.GetVerticalCollision(player.Position,MathHelper.ToRadians( player.ViewingAngle), 10);
            //Assert
            Assert.AreEqual(Vector2.One * 2, collisionPoint.Value, "The two values are not the same");

            //Act
            player.ViewingAngle = 0;
            collisionPoint = map.GetVerticalCollision(player.Position, MathHelper.ToRadians(player.ViewingAngle), 10);
            //Assert
            Assert.AreEqual(new Vector2(2,player.Position.Y), collisionPoint.Value, "The two values are not the same");

            //Act
            player.Position = Vector2.One * 1.25f;
            player.ViewingAngle = 180;
            collisionPoint = map.GetVerticalCollision(player.Position, MathHelper.ToRadians(player.ViewingAngle), 10);
            //Assert
            Assert.AreEqual(new Vector2(1,1.25f), collisionPoint.Value, "The two values are not the same");
        }

        [TestMethod]
        public void TestWholeGridCollisionFinderHorizontally()
        {
            //Arrange
            Player player = new Raycasting.Player(map);
            player.Position = new Vector2(1.5f, 1.5f);
            player.ViewingAngle = 45;

            //Act
            var collisionPoint = map.GetHorizontalCollision(player.Position, MathHelper.ToRadians(player.ViewingAngle), 10);
            //Assert
            Assert.AreEqual(new Vector2(2,1), collisionPoint.Value, "The two values are not the same");

            //Act
            player.ViewingAngle = 90;
            collisionPoint = map.GetHorizontalCollision(player.Position, MathHelper.ToRadians(player.ViewingAngle), 10);
            //Assert
            Assert.AreEqual(new Vector2(1.5f, 1), collisionPoint.Value, "The two values are not the same");

            //Act
            player.Position = Vector2.One * 1.25f;
            player.ViewingAngle = 315;
            collisionPoint = map.GetHorizontalCollision(player.Position, MathHelper.ToRadians(player.ViewingAngle), 10);
            //Assert
            //Assert.IsTrue(Vector2.Distance(collisionPoint.Value, new Vector2(2, 2)) < .0001, "The two values are not the same");
        }


        [TestMethod]
        public void TestTangent()
        {
            //Arrange 
            float slope = (float)Math.Tan((float)Math.PI/4);
            //Assert
            Assert.AreEqual(1, slope);
        }

        [TestMethod]
        public void TestFindingYIntersect()
        {
            //arrange 
            float a = 1;
            Vector2 pointOnLine = Vector2.One * 2;
            //act
            float intersect = LineFormula.FindYIntersect(a, pointOnLine);
            //assert
            Assert.IsTrue(intersect == 0, "Not intersecting in origo!");
            
            
            //arrange 
            a = 2;
            pointOnLine = new Vector2(1,3);
            //act
            intersect = LineFormula.FindYIntersect(a, pointOnLine);
            //assert
            Assert.IsTrue(intersect == 1, "Not intersecting in origo!");


            //arrange 
            a = -3;
            pointOnLine = new Vector2(1, 0);
            //act
            intersect = LineFormula.FindYIntersect(a, pointOnLine);
            //assert
            Assert.IsTrue(intersect == 3, "Not intersecting in origo!");
        }

        [TestMethod]
        public void TestLineFormula()
        {
            //arrange 
            float angleInRadians = (float)Math.PI / 4;
            Vector2 direction = angleInRadians.AngleAsVector();
            var knownPositionOnLine = Vector2.One;
            //act
            var line = LineFormula.FromCoordinateAndDirection(knownPositionOnLine, direction);
            var line2 = LineFormula.FromCoordinateAndDirection(knownPositionOnLine, angleInRadians);
            //assert
            Assert.IsTrue(line.B == 0, "Not intersecting in origo!");
            Assert.AreEqual(line, line2, "Not getting same line objects from constructors");

        }


        [TestMethod]
        public void TestLineIntersectWithHorizontalLines()
        {
            //arrange 
            Vector2 direction = new Vector2(3, -2);
            Vector2 knownPositionOnLine = new Vector2(3, 2);
            var line = LineFormula.FromCoordinateAndDirection(knownPositionOnLine, direction);
            float horizontalLinesYValue = 4;
            //act

            float xValueOfIntersectWithHorizontalLine = line.GetInterSectWithHorizontalLine(horizontalLinesYValue).Value;
            
            //assert
            Assert.IsTrue(xValueOfIntersectWithHorizontalLine == 0, "Not intersecting in origo!");

        }


        [TestMethod]
        public void TestLineIntersectWithVerticalLines()
        {
            //arrange 
            Vector2 direction = new Vector2(3, -2);
            Vector2 knownPositionOnLine = new Vector2(3, 2);
            var line = LineFormula.FromCoordinateAndDirection(knownPositionOnLine, direction);
            float verticalLinesXValue = -3;
            //act

            float yValueOfIntersectWithVerticalLine= line.GetInterSectWithVerticalLine(verticalLinesXValue).Value;

            //assert
            Assert.IsTrue(yValueOfIntersectWithVerticalLine == 6, "Not intersecting in origo!");

        }

    }
}
