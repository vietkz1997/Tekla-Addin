
template _tmp_0
{
    name = "tpled_template1";
    type = GRAPHICAL;
    width = 200;
    maxheight = 287;
    columns = (1, 1);
    gap = 5;
    fillpolicy = EVEN;
    filldirection = HORIZONTAL;
    fillstartfrom = TOPLEFT;
    margins = (0, 0, 0, 0);
    gridxspacing = 1;
    gridyspacing = 1;
    version = 3.6;
    created = "14.08.2018 00:30";
    modified = "03.05.2019 15:39";
    notes = "";

    row _tmp_2
    {
        name = "Outline";
        height = 287;
        visibility = TRUE;
        usecolumns = FALSE;
        rule = "";
        contenttype = "";
        sorttype = COMBINE;

        group _tmp_5
        {
            name = "Group";

            lineorarc _tmp_3
            {
                name = "LineOrArc_4";
                x1 = 195;
                y1 = 287;
                x2 = 200;
                y2 = 287;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_4
            {
                name = "LineOrArc_5";
                x1 = 200;
                y1 = 282;
                x2 = 200;
                y2 = 287;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };

        polyline _tmp_9
        {
            name = "Polyline";
            filled = FALSE;
            filltype = -1;
            color = 153;
            linetype = 1;
            linewidth = 1;
            pen = -1;

            lineorarc _tmp_10
            {
                name = "LineOrArc_4";
                x1 = 0;
                y1 = 5;
                x2 = 0;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };

            lineorarc _tmp_11
            {
                name = "LineOrArc_4";
                x1 = 0;
                y1 = 0;
                x2 = 5;
                y2 = 0;
                pen = -1;
                color = 153;
                linetype = 1;
                linewidth = 1;
                bulge = 0;
            };
        };
    };
};
