template _tmp_839
{
    name = "template1";
    type = GRAPHICAL;
    width = 68;
    maxheight = 120;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    fillstartfrom = TOPLEFT;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.6;
    created = "29.12.2008 15:55";
    modified = "18.07.2018 09:36";
    notes = "";

    row _tmp_865
    {
        name = "PART";
        height = 14;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "PART";
        sorttype = COMBINE;

        text _tmp_899
        {
            name = "DRAWING TITLE :";
            x1 = 1.11325245311002;
            y1 = 8.82130222765775;
            x2 = 1.11325245311002;
            y2 = 8.82130222765775;
            string = "DRAWING TITLE :";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 0.8;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        text _tmp_900
        {
            name = "DRAWING NO :";
            x1 = 1.11325245311002;
            y1 = 1.7872738543244;
            x2 = 1.11325245311002;
            y2 = 1.7872738543244;
            string = "DRAWING NO. :";
            fontname = "romsim";
            fontcolor = 153;
            fonttype = 4;
            fontsize = 3;
            fontratio = 0.8;
            fontslant = 0;
            fontstyle = 0;
            angle = 0;
            justify = LEFT;
            pen = -1;
        };

        valuefield _tmp_909
        {
            name = "DRAWING.TITLE_field";
            location = (35.7047410280679, 8.94658468599109);
            formula = "GetValue(\"DRAWING.TITLE\")";
            maxnumoflines = 1;
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            formatzeroasempty = FALSE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 12;
            decimals = 0;
            sortdirection = ASCENDING;
            fontname = "Arial";
            fontcolor = 161;
            fonttype = 2;
            fontsize = 3;
            fontratio = 0.8;
            fontstyle = 0;
            fontslant = 0;
            pen = -1;
            oncombine = NONE;
        };

        valuefield _tmp_910
        {
            name = "DR_PART_POS_field";
            location = (35.7047410280679, 1.91255631265773);
            formula = "GetValue(\"PART_POS\")";
            datatype = STRING;
            class = "";
            cacheable = TRUE;
            justify = LEFT;
            visibility = TRUE;
            angle = 0;
            length = 7;
            decimals = 0;
            sortdirection = ASCENDING;
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

        lineorarc _tmp_867
        {
            name = "LineOrArc_i1";
            x1 = 68;
            y1 = 7;
            x2 = 0;
            y2 = 7;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_868
        {
            name = "LineOrArc_i2";
            x1 = 68;
            y1 = 14;
            x2 = 0;
            y2 = 14;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_872
        {
            name = "LineOrArc_i6";
            x1 = 68;
            y1 = 0;
            x2 = 0;
            y2 = 0;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_1031
        {
            name = "LineOrArc";
            x1 = 68;
            y1 = 0;
            x2 = 68;
            y2 = 14;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };

        lineorarc _tmp_1035
        {
            name = "LineOrArc_1";
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 14;
            pen = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            bulge = 0;
        };
    };
};
