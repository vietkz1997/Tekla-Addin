
template _tmp_1529
{
    name = "tpled_template2";
    type = GRAPHICAL;
    width = 138.170528388586;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.21;
    created = "08.12.2008 11:55";
    modified = "11.06.2013 17:50";
    notes = "";

    row _tmp_1556
    {
        name = "REVISION";
        height = 7.1644;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "if GetValue(\"NUMBER\")>GetValue(\"LAST\")-5 then\r\n  Output()\r\nelse\r\n  StepOver()\r\nendif";
        contenttype = "REVISION";
        sorttype = COMBINE;

        lineorarc _tmp_1565
        {
            name = "LineOrArc_i8";
            x1 = 0;
            y1 = 7.16439776829705;
            x2 = 0;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_1566
        {
            name = "LineOrArc_i9";
            x1 = 0;
            y1 = 0;
            x2 = 138.170528388586;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_1568
        {
            name = "LineOrArc_i11";
            x1 = 0;
            y1 = 7.16439776829705;
            x2 = 138.170528388586;
            y2 = 7.16439776829705;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        valuefield _tmp_2079
        {
            name = "Date";
            location = (8, 2);
            formula = "GetValue(\"DATE_CREATE\")";
            datatype = INTEGER;
            class = "Date";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 20;
            decimals = 0;
            sortdirection = DESCENDING;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 3;
            fontratio = 0.8;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
            unit = "yyyy.mm.dd";
        };

        valuefield _tmp_2224
        {
            name = "text1";
            location = (31, 2);
            formula = "GetValue(\"DESCRIPTION\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 10;
            decimals = 0;
            sortdirection = DESCENDING;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 3;
            fontratio = 0.8;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_2749
        {
            name = "text2";
            location = (70, 2);
            formula = "GetValue(\"INFO1\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 5;
            decimals = 0;
            sortdirection = DESCENDING;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 3;
            fontratio = 0.8;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_2787
        {
            name = "text3";
            location = (81, 2);
            formula = "GetValue(\"INFO2\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 6;
            decimals = 0;
            sortdirection = DESCENDING;
            fontname = "romsim";
            fontcolor = 161;
            fonttype = 4;
            fontsize = 3;
            fontratio = 0.8;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };
    };

    pagefooter _tmp_870
    {
        name = "PageFooter";
        height = 14.3287955365941;
        outputpolicy = NONE;

        lineorarc _tmp_882
        {
            name = "LineOrArc";
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 14.3287955365941;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_883
        {
            name = "LineOrArc_1";
            x1 = 0;
            y1 = 14.3287955365941;
            x2 = 138.170528388586;
            y2 = 14.3287955365941;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };
    };
};
