using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    using System.Diagnostics;

    [TestClass()]
    public class BitboardsTests
    {
        [TestMethod()]
        public void initTest()
        {
            Bitboards.init();

            // check AdjacentFilesBB values
            Assert.AreEqual(144680345676153346UL, Utils.AdjacentFilesBB[0]);
            Assert.AreEqual(361700864190383365UL, Utils.AdjacentFilesBB[1]);
            Assert.AreEqual(723401728380766730UL, Utils.AdjacentFilesBB[2]);
            Assert.AreEqual(1446803456761533460UL, Utils.AdjacentFilesBB[3]);
            Assert.AreEqual(2893606913523066920UL, Utils.AdjacentFilesBB[4]);
            Assert.AreEqual(5787213827046133840UL, Utils.AdjacentFilesBB[5]);
            Assert.AreEqual(11574427654092267680UL, Utils.AdjacentFilesBB[6]);
            Assert.AreEqual(4629771061636907072UL, Utils.AdjacentFilesBB[7]);

            // check some distance field
            Assert.AreEqual(2, Utils.SquareDistance[(int)Square.SQ_A1, (int)Square.SQ_B3]);
            Assert.AreEqual(7, Utils.SquareDistance[(int)Square.SQ_A8, (int)Square.SQ_H1]);

            // check some RookMasks fields
            Assert.AreEqual(282578800148862UL, Utils.RookMasks[0]);
            Assert.AreEqual(565157600297596UL, Utils.RookMasks[1]);
            Assert.AreEqual(1130315200595066UL, Utils.RookMasks[2]);
            Assert.AreEqual(2260630401190006UL, Utils.RookMasks[3]);
            Assert.AreEqual(4521260802379886UL, Utils.RookMasks[4]);
            Assert.AreEqual(9042521604759646UL, Utils.RookMasks[5]);
            Assert.AreEqual(18085043209519166UL, Utils.RookMasks[6]);
            Assert.AreEqual(36170086419038334UL, Utils.RookMasks[7]);

            // check some RookMagics fields
            Assert.AreEqual(1225049467397373984UL, Utils.RookMagics[0]);
            Assert.AreEqual(1225049467397373984UL, Utils.RookMagics[1]);
            Assert.AreEqual(9018266856982672UL, Utils.RookMagics[2]);
            Assert.AreEqual(74873168447144976UL, Utils.RookMagics[3]);
            Assert.AreEqual(10412331411607454724UL, Utils.RookMagics[4]);

            // check some RookAttacks fields
            Assert.AreEqual(72340172838076926UL, Utils.RookAttacks[0][0]);
            Assert.AreEqual(4311810306UL, Utils.RookAttacks[0][1]);

            // check some RookShifts fields
            Assert.AreEqual(20u, Utils.RookShifts[0]);
            Assert.AreEqual(21u, Utils.RookShifts[1]);
            Assert.AreEqual(21u, Utils.RookShifts[2]);

            // check SquareBB values
            Assert.AreEqual(8589934592ul, Utils.SquareBB[33]);
            Assert.AreEqual(17179869184ul, Utils.SquareBB[34]);
            Assert.AreEqual(34359738368ul, Utils.SquareBB[35]);
            Assert.AreEqual(68719476736ul, Utils.SquareBB[36]);

            // check FileBB values
            Assert.AreEqual(72340172838076673ul, Utils.FileBB[0]);
            Assert.AreEqual(144680345676153346ul, Utils.FileBB[1]);
            Assert.AreEqual(289360691352306692ul, Utils.FileBB[2]);

            // check RankBB values
            Assert.AreEqual(280375465082880ul, Utils.RankBB[5]);
            Assert.AreEqual(71776119061217280ul, Utils.RankBB[6]);
            Assert.AreEqual(18374686479671623680ul, Utils.RankBB[7]);

            // check InFrontBB values
            Assert.AreEqual(18446744073709551360ul, Utils.InFrontBB[0, 0]);
            Assert.AreEqual(18446744073709486080ul, Utils.InFrontBB[0, 1]);
            Assert.AreEqual(0ul, Utils.InFrontBB[1, 0]);
            Assert.AreEqual(255ul, Utils.InFrontBB[1, 1]);

            // check StepAttacksBB values
            Assert.AreEqual(132096ul, Utils.StepAttacksBB[10, 0]);
            Assert.AreEqual(329728ul, Utils.StepAttacksBB[10, 1]);
            Assert.AreEqual(770ul, Utils.StepAttacksBB[14, 0]);
            Assert.AreEqual(1797ul, Utils.StepAttacksBB[14, 1]);

            // check BetweenBB values
            Assert.AreEqual(134480384ul, Utils.BetweenBB[36, 0]);
            Assert.AreEqual(0ul, Utils.BetweenBB[36, 1]);
            Assert.AreEqual(0ul, Utils.BetweenBB[37, 0]);
            Assert.AreEqual(268960768ul, Utils.BetweenBB[37, 1]);

            // check LineBB values
            Assert.AreEqual(72340172838076673ul, Utils.LineBB[8, 0]);
            Assert.AreEqual(258ul, Utils.LineBB[8, 1]);
            Assert.AreEqual(9241421688590303745ul, Utils.LineBB[9, 0]);
            Assert.AreEqual(144680345676153346ul, Utils.LineBB[9, 1]);

            // check DistanceRingBB values
            Assert.AreEqual(1884319744ul, Utils.DistanceRingBB[21, 0]);
            Assert.AreEqual(1067442538744ul, Utils.DistanceRingBB[21, 1]);
            Assert.AreEqual(3768639488ul, Utils.DistanceRingBB[22, 0]);
            Assert.AreEqual(1031061639408ul, Utils.DistanceRingBB[22, 1]);

            // check ForwardBB values
            Assert.AreEqual(72340172838076672ul, Utils.ForwardBB[0, 0]);
            Assert.AreEqual(144680345676153344ul, Utils.ForwardBB[0, 1]);
            Assert.AreEqual(137977929760ul, Utils.ForwardBB[1, 45]);
            Assert.AreEqual(275955859520ul, Utils.ForwardBB[1, 46]);

            // check PassedPawnMask values
            Assert.AreEqual(217020518514230016ul, Utils.PassedPawnMask[0, 0]);
            Assert.AreEqual(506381209866536704ul, Utils.PassedPawnMask[0, 1]);
            Assert.AreEqual(235802126ul, Utils.PassedPawnMask[1, 34]);
            Assert.AreEqual(471604252ul, Utils.PassedPawnMask[1, 35]);

            // check PawnAttackSpan values
            Assert.AreEqual(144680345676153344ul, Utils.PawnAttackSpan[0, 0]);
            Assert.AreEqual(361700864190383360ul, Utils.PawnAttackSpan[0, 1]);
            Assert.AreEqual(176611750092960ul, Utils.PawnAttackSpan[1, 54]);
            Assert.AreEqual(70644700037184ul, Utils.PawnAttackSpan[1, 55]);

            // check PseudoAttacks values
            Assert.AreEqual(9241421688590303744ul, Utils.PseudoAttacks[3, 0]);
            Assert.AreEqual(36099303471056128ul, Utils.PseudoAttacks[3, 1]);
            Assert.AreEqual(72340172838076926ul, Utils.PseudoAttacks[4, 0]);
            Assert.AreEqual(144680345676153597ul, Utils.PseudoAttacks[4, 1]);

            // check some MSBTable fields
            Assert.AreEqual(5, Utils.MSBTable[63]);
            Assert.AreEqual(6, Utils.MSBTable[64]);

            // check some BSFTable fields
            Assert.AreEqual(Square.SQ_G7, Utils.BSFTable[27]);
            Assert.AreEqual(Square.SQ_B2, Utils.BSFTable[28]);
            Assert.AreEqual(Square.SQ_B8, Utils.BSFTable[29]);
        }
    }
}


