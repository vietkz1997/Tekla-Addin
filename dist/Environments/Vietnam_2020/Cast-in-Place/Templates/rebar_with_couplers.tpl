
template _tmp_0
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 150;
    maxheight = 300;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (10, 10, 10, 10);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.21;
    created = "25.05.2015 13:18";
    modified = "20.06.2016 12:50";
    notes = "";

    header _tmp_1
    {
        name = "Header";
        height = 15;

        text _tmp_2
        {
            name = "Rebars with couplers";
            x1 = 5;
            y1 = 10;
            x2 = 5;
            y2 = 10;
            string = "Rebars with couplers";
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_4
        {
            name = "Pos";
            x1 = 5;
            y1 = 5;
            x2 = 5;
            y2 = 5;
            string = "Pos";
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_5
        {
            name = "Size";
            x1 = 25;
            y1 = 5;
            x2 = 25;
            y2 = 5;
            string = "Size";
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_6
        {
            name = "Number";
            x1 = 43;
            y1 = 5;
            x2 = 43;
            y2 = 5;
            string = "Number";
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        rectangle _tmp_2
        {
            name = "Rectangle_1";
            x1 = 0;
            y1 = 15;
            x2 = 130;
            y2 = 4;
            filled = FALSE;
            filltype = -1;
            pen = -1;
            color = 161;
            linetype = 1;
            linewidth = 1;
        };

        lineorarc _tmp_3
        {
            name = "LineOrArc_3";
            x1 = 0;
            y1 = 9;
            x2 = 130;
            y2 = 9;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_6
        {
            name = "LineOrArc_5";
            x1 = 40;
            y1 = 4;
            x2 = 40;
            y2 = 9;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_9
        {
            name = "LineOrArc_4";
            x1 = 20;
            y1 = 4;
            x2 = 20;
            y2 = 9;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_10
        {
            name = "LineOrArc_6";
            x1 = 55;
            y1 = 4;
            x2 = 55;
            y2 = 9;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        text _tmp_11
        {
            name = "Shape";
            x1 = 57;
            y1 = 5;
            x2 = 57;
            y2 = 5;
            string = "Shape";
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };
    };

    row _tmp_3
    {
        name = "REBAR";
        height = 20;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "REBAR";
        sorttype = COMBINE;

        lineorarc _tmp_7
        {
            name = "LineOrArc";
            x1 = 20;
            y1 = 13;
            x2 = 20;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_8
        {
            name = "LineOrArc_1";
            x1 = 40;
            y1 = 13;
            x2 = 40;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_9
        {
            name = "LineOrArc_2";
            x1 = 55;
            y1 = 13;
            x2 = 55;
            y2 = 0;
            pen = -1;
            color = 164;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        valuefield _tmp_11
        {
            name = "SIZE_field";
            location = (25, 8);
            formula = "GetValue(\"SIZE\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 4;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1.5;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_13
        {
            name = "ValueField";
            location = (45.8984375, 8);
            formula = "GetValue(\"NUMBER\")";
            datatype = INTEGER;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 4;
            decimals = 0;
            sortdirection = NONE;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1.5;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = SUM;
        };

        graphicalfield _tmp_14
        {
            name = "GraphicalField";
            location = (56, 1);
            field = "CUSTOM.REBAR_SHAPE_COUPLERS";
            height = 18;
            width = 72;
        };

        rectangle _tmp_1
        {
            name = "Rectangle";
            x1 = 0;
            y1 = 20;
            x2 = 130;
            y2 = 0;
            filled = FALSE;
            filltype = -1;
            pen = -1;
            color = 161;
            linetype = 1;
            linewidth = 1;
        };

        valuefield _tmp_0
        {
            name = "REBAR_POS";
            location = (1, 8);
            formula = "GetValue(\"REBAR_POS\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 8;
            decimals = 1;
            sortdirection = ASCENDING;
            fontname = "Arial Narrow";
            fontcolor = 153;
            fonttype = 2;
            fontsize = 2.5;
            fontratio = 1.5;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };
};
