
template _tmp_897
{
    name = "template3";
    type = GRAPHICAL;
    width = 93.506724357605;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.21;
    created = "07.07.2007 09:24";
    modified = "24.05.2011 17:53";
    notes = "";

    row _tmp_922
    {
        name = "ONE_PART";
        height = 5;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "PART";
        sorttype = COMBINE;

        valuefield _tmp_988
        {
            name = "1";
            location = (0, 0);
            formula = "GetValue(\"MODEL_TOTAL\")+\" - \"+GetValue(\"NAME\")+\" - \"+GetValue(\"PART_POS\")\r\n\r\n ";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = CENTERED;
            visibility = TRUE;
            angle = 0;
            length = 27;
            decimals = 0;
            sortdirection = NONE;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 5;
            fontratio = 0.8;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };
};
