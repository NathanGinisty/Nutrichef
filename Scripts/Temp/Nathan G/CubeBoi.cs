using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBoi : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] float timer;
    [SerializeField] float t_max;
    private float t_actual;

    private GameObject goTest1;
    private GameObject pommie;

    private Previsualization prev;
    private SelectionFrame selFrame;

    private string strPavéCaesar;
    private string strPavéMacaron;
    private GameObject selectInfo;

    void Start()
    {
        GameManager.Instance.FreeMouse();

        FoodDatabase.InstantiateAliment("Pomme", AlimentState.Standard);
        pommie = FoodDatabase.InstantiateAliment("Pomme", AlimentState.Standard);
        pommie.transform.position = new Vector3(0, 5, 0);
        pommie.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        //goTest1 = FoodDatabase.InstantiateCardboardBox(100f, FoodDatabase.mapAlimentObject["Pomme"]);

        t_actual = 0f;

        // no player so can't follow cam
        //GameManager.Instance.PopUp.CreateText3D("AYAYA", 80, Vector3.up, transform);
        //GameManager.Instance.PopUp.CreatePrefab3D("cursor", Vector2.one, Vector3.up * 2, transform);
        //GameManager.Instance.PopUp.CreatePrefab3D("ParticleTest", Vector2.one, Vector3.up * 2, transform);

        //Object tmpObjectPrev = Resources.Load<Object>("Temp/1");//Resources.Load<Object>("Walls/M_Door_Basic_Combined");
        //Object tmpObjectPrev = Resources.Load<Object>("Walls/M_Door_Basic_Combined");
        //GameObject GOPrevtmp = Instantiate<GameObject>(tmpObjectPrev as GameObject);

        prev = GameManager.Instance.PopUp.CreatePrevisualization(transform, true, true);
        prev.Initialise();
        prev.SetPrevisualization(pommie.GetComponentInChildren<MeshFilter>().mesh, pommie.GetComponentInChildren<MeshRenderer>().materials);
        prev.SetLerp(true, 0.1f);

        selFrame = GameManager.Instance.PopUp.CreateSelectionFrame(transform, Vector3.zero, Vector3.one);

        // giant string
        {
            strPavéCaesar = "On remarque d’abord qu’un certain nombre de personnes se trouvent entre une situation d’inactivité et de chômage (cf. zone 3)." +
                "Parmi elles, beaucoup désirent travailler mais ne sont pas comptabilisées parce qu’elles ont trop peu de chance de retrouver un emploi (et" +
                " sont donc dispensées de recherche d’emploi) ou parce qu’elles ont renoncé, par découragement, à rechercher un emploi. Dans ce dernier cas" +
                ", il peut s’agir de chômeurs de longue durée subissant des cas d’extrême exclusion sociale, de mères au foyer désirant travailler mais n’e" +
                "ntamant pas de démarche, ou encore d’étudiants choisissant de poursuivre leurs études à défaut d’avoir pu se faire embaucher. ";

            strPavéMacaron = " Lorem ipsum dolor sit amet, consectetur adipiscing elit.Sed a sem vel elit euismod gravida.Donec vulputate efficitur porta. " +
                "Vivamus rutrum lacinia maximus. Suspendisse ut odio at justo imperdiet tincidunt non quis augue. Donec sit amet orci tincidunt, pharetra mi " +
                "suscipit, mattis turpis. In aliquet, augue a sagittis consequat, diam risus convallis massa, sed cursus ligula enim non nibh.Proin cursus " +
                "gravida purus laoreet ullamcorper. Sed convallis tempor nunc, ac viverra ipsum ornare eu. Donec et ex magna. Donec varius dapibus rutrum. " +
                "Aliquam erat lectus, consectetur non luctus sit amet, convallis at libero.Mauris iaculis elementum fringilla. Aliquam erat volutpat.Phasellus " +
                "a dolor sit amet mi iaculis dictum. Suspendisse laoreet eu ligula non consectetur. Donec ac facilisis tellus. Nullam in dapibus ipsum, aliquam" +
                " eleifend nulla. Phasellus facilisis faucibus velit, ut ultricies est. Ut ut congue dui. Nunc vel tortor ac ex vulputate sollicitudin ac vitae " +
                "sem. Vivamus quam arcu, viverra sit amet efficitur et, finibus a tellus.Aenean vestibulum congue enim, vel rutrum elit condimentum ut. Vestibulum" +
                " ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Sed elementum at arcu ultricies dignissim. Maecenas ac erat in ex" +
                " consequat tempus ut in urna.Fusce vulputate sed nibh quis condimentum. Etiam mi enim, rutrum ut fringilla vitae, vehicula sed urna. Praesent " +
                "imperdiet a odio in laoreet.Pellentesque porta dolor id metus pharetra, quis bibendum sapien ultricies.Integer ut diam magna. Cras bibendum b" +
                "ibendum sapien sit amet placerat.Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aenean aliquam lacinia" +
                " nulla vitae pharetra. Cras scelerisque orci et finibus eleifend. Ut in magna eget quam blandit condimentum.Phasellus luctus eget erat sed " +
                "cursus. Nulla ac sapien iaculis, dictum nisi quis, scelerisque eros. Lorem ipsum dolor sit amet, consectetur adipiscing elit.Sed accumsan in justo" +
                " mollis bibendum.Nam cursus dui magna, at gravida erat auctor at. Fusce eu nunc elit. Donec id suscipit eros. Maecenas sed varius mi. Praesent sit " +
                "amet dui feugiat nunc pellentesque condimentum. Integer ut turpis consectetur, porttitor eros faucibus, auctor diam.Maecenas ut auctor dolor, in" +
                " iaculis mauris. Morbi vel tincidunt ipsum. Curabitur varius diam ante, non tincidunt lorem vestibulum sit amet. Curabitur nibh sem, tempor a " +
                "accumsan quis, tincidunt eget ligula. Vestibulum non turpis iaculis, sagittis justo et, accumsan sapien.Integer commodo elit dui. Praesent dapibus" +
                " tempor vestibulum. Cras a turpis et arcu hendrerit hendrerit euismod vitae justo. Fusce sagittis euismod ante, eu vehicula augue fermentum ac." +
                " Nunc viverra urna tempus pulvinar rutrum. Sed orci mauris, bibendum sed justo at, iaculis tincidunt augue. Curabitur quis auctor tortor. Aliquam " +
                "dapibus lorem elit, eu semper diam elementum a. Sed malesuada neque ac elementum luctus. Duis tempor, urna ut dictum euismod, sem diam cursus eros, in" +
                " aliquet arcu nulla non leo.Fusce nisl metus, sagittis ac nibh ut, rutrum facilisis enim. Duis et nunc lobortis, sollicitudin odio et, vulputate purus. ";
        }
    }

    void Update()
    {
        // Open Menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.InGameMenu.transform.GetChild(0).gameObject.SetActive(true);
        }

        // update, test textWindow
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                prev.SetPos(hit.point);

                //selFrame.SetSize(hit.point);

                //Debug.Log(hit.collider.gameObject.name);

                if (hit.collider.name.Contains("Bowl"))
                {
                    if (selectInfo == null)
                    {
                        //selectInfo = GameManager.Instance.PopUp.CreateTextWindow(strPavéMacaron , 30, Input.mousePosition);
                        selectInfo = GameManager.Instance.PopUp.CreateTextWindow(strPavéCaesar, new Vector2(450, 800), Input.mousePosition);
                    }
                    //selectInfo.transform.position = Input.mousePosition;
                }
                else
                {
                    if (selectInfo != null) Destroy(selectInfo);
                }
            }
        }        

        // clic, test Previsualization
        if (Input.GetMouseButtonDown(0))
        {
            prev.SetPrevisualization(pommie.GetComponentInChildren<MeshFilter>().mesh);

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                //prev.SetPos(hit.point);
            }

            //Debug.Log(GameManager.Instance.Score.UpdateNoteFinal());

            //FoodDatabase.InstantiateStackFromCardboardBox(goTest1);
        }

        t_actual += Time.deltaTime;
        if (t_actual > t_max)
        {
            //GameManager.Instance.Audio.PlaySound("PUNCH", AudioManager.Canal.Ambient, gameObject);
            //GameManager.Instance.PopUp.CreateText("yolo", 400, new Vector2(400, 400), 1f, "FadeToTransparent");
            //GameManager.Instance.PopUp.CreatePrefab("image2D", new Vector2(300, 100), new Vector2(-400, -400), 1f);
            //GameManager.Instance.Score.myScore.AddError(Score.BuilderCounter., 0f, );

            t_actual = 0f;
        }
        timer = t_actual / t_max;
    }
}
