static public string ConvertToStandardNotation(int x, int y)
        {
            x = 5 - x;

            string notation = x.ToString() + letters[y];

            return notation;
        }


        static public Position ConvertToBasic(string notation)
        {
            Position position = new Position();
            int row;
            //-1 is a bogus value to satiate the compiler
            int Column = -1;
            string letter;
            row = (int)Char.GetNumericValue(notation[0]);
            letter = notation[1].ToString();

            for (int i = 0; i < letters.Count; i++)
            {
                if (letters[i] == letter)
                {
                    Column = i;
                }
            }

            row = CreeperBoard.PegRows - 1 - row;
            position.Column = row;
            position.Row = Column;
            return position;
        }